using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    private readonly EmpresaService _empresaService;
    private readonly SucursalService _sucursalService;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UsuarioService usuarioService,
        EmpresaService empresaService,
        SucursalService sucursalService,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthController> logger)
    {
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _empresaService = empresaService ?? throw new ArgumentNullException(nameof(empresaService));
        _sucursalService = sucursalService ?? throw new ArgumentNullException(nameof(sucursalService));
        _jwtSettings = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegistroEmprendedorRequest
    {
        public string NombreEmpresa { get; set; } = null!;
        public string Rubro { get; set; } = null!;
        public string DescripcionEmpresa { get; set; } = null!;
        public decimal MargenGanancia { get; set; }
        public string? LogoUrl { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? NombreSucursal { get; set; }
        public string? DireccionSucursal { get; set; }
        public string? CiudadSucursal { get; set; }
        public string? DepartamentoSucursal { get; set; }
    }

    public class CrearTrabajadorRequest
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? SucursalId { get; set; }
        public string? TrabajadorId { get; set; }
    }

    [HttpGet("test")]
    [AllowAnonymous]
    public ActionResult<object> Test()
    {
        return Ok(new { message = "API funcionando correctamente", timestamp = DateTime.UtcNow });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Email y password son requeridos.");

            var usuario = await _usuarioService.GetByEmailAndPasswordAsync(
                request.Email, request.Password);

            if (usuario is null)
                return Unauthorized("Credenciales inválidas.");

            if (string.IsNullOrEmpty(_jwtSettings.Key))
                return StatusCode(500, "Error de configuración del servidor: JWT Key no configurada.");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id!),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.TipoUsuario),
                new Claim("tipoUsuario", usuario.TipoUsuario),
                new Claim("empresaId", usuario.EmpresaId)
            };

            if (!string.IsNullOrEmpty(usuario.SucursalId))
            {
                claims.Add(new Claim("sucursalId", usuario.SucursalId));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.TipoUsuario,
                    usuario.EmpresaId,
                    usuario.SucursalId,
                    usuario.TrabajadorId
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Error interno del servidor", mensaje = ex.Message });
        }
    }

    [HttpPost("registro-emprendedor")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> RegistroEmprendedor([FromBody] RegistroEmprendedorRequest? request)
    {
        _logger.LogInformation("=== INICIO REGISTRO EMPRENDEDOR ===");
        
        try
        {
            // 1. VALIDAR REQUEST
            if (request == null)
            {
                _logger.LogWarning("Request es null");
                return BadRequest(new { 
                    error = "Request inválido", 
                    message = "El cuerpo de la petición no puede estar vacío" 
                });
            }

            _logger.LogInformation("Request recibido. Email: {Email}, NombreEmpresa: {NombreEmpresa}", 
                request.Email ?? "null", request.NombreEmpresa ?? "null");

            // Validar ModelState (errores de binding)
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => new { campo = x.Key, error = e.ErrorMessage }))
                    .ToList();

                _logger.LogWarning("ModelState inválido: {Errors}", string.Join(", ", errors.Select(e => $"{e.campo}: {e.error}")));
                
                return BadRequest(new
                {
                    error = "Datos inválidos",
                    message = "El formato de los datos enviados es incorrecto",
                    errors = errors
                });
            }

            _logger.LogInformation("Iniciando registro de emprendedor para email: {Email}", request.Email ?? "null");

            // Validar campos requeridos
            var camposFaltantes = new List<string>();
            
            if (string.IsNullOrWhiteSpace(request.Email))
                camposFaltantes.Add("email");
            
            if (string.IsNullOrWhiteSpace(request.Password))
                camposFaltantes.Add("password");
            
            if (string.IsNullOrWhiteSpace(request.NombreEmpresa))
                camposFaltantes.Add("nombreEmpresa");
            
            if (string.IsNullOrWhiteSpace(request.NombreUsuario))
                camposFaltantes.Add("nombreUsuario");
            
            if (camposFaltantes.Any())
            {
                _logger.LogWarning("Campos requeridos faltantes: {Campos}", string.Join(", ", camposFaltantes));
                return BadRequest(new { 
                    error = "Campos requeridos faltantes", 
                    message = $"Los siguientes campos son requeridos: {string.Join(", ", camposFaltantes)}",
                    camposFaltantes = camposFaltantes
                });
            }

            // 2. VALIDAR EMAIL ÚNICO
            _logger.LogInformation("Verificando si el email {Email} ya existe", request.Email);
            var usuarioExistente = await _usuarioService.GetByEmailAsync(request.Email);
            if (usuarioExistente != null)
            {
                _logger.LogWarning("Intento de registro con email duplicado: {Email}", request.Email);
                return Conflict(new { 
                    error = "Email ya registrado", 
                    message = $"El email '{request.Email}' ya está registrado en el sistema" 
                });
            }

            // 3. CREAR EMPRESA
            _logger.LogInformation("Creando empresa: {NombreEmpresa}", request.NombreEmpresa);
            var empresa = new Empresa
            {
                Nombre = request.NombreEmpresa.Trim(),
                Rubro = string.IsNullOrWhiteSpace(request.Rubro) ? string.Empty : request.Rubro.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(request.DescripcionEmpresa) ? string.Empty : request.DescripcionEmpresa.Trim(),
                MargenGanancia = request.MargenGanancia,
                LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? string.Empty : request.LogoUrl.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _empresaService.CreateAsync(empresa);

            // Verificar que la empresa se creó correctamente
            if (string.IsNullOrEmpty(empresa.Id))
            {
                _logger.LogError("Error crítico: La empresa se creó pero no se generó el ID");
                return StatusCode(500, new { 
                    error = "Error al crear la empresa", 
                    message = "No se pudo generar el ID de la empresa. Por favor, intente nuevamente." 
                });
            }

            _logger.LogInformation("Empresa creada exitosamente con ID: {EmpresaId}", empresa.Id);

            // 4. CREAR SUCURSAL (OPCIONAL)
            string? sucursalId = null;
            if (!string.IsNullOrWhiteSpace(request.NombreSucursal))
            {
                _logger.LogInformation("Creando sucursal: {NombreSucursal} para empresa {EmpresaId}", 
                    request.NombreSucursal, empresa.Id);

                var sucursal = new Sucursal
                {
                    EmpresaId = empresa.Id,
                    Nombre = request.NombreSucursal.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(request.DireccionSucursal) ? string.Empty : request.DireccionSucursal.Trim(),
                    Ciudad = string.IsNullOrWhiteSpace(request.CiudadSucursal) ? string.Empty : request.CiudadSucursal.Trim(),
                    Departamento = string.IsNullOrWhiteSpace(request.DepartamentoSucursal) ? string.Empty : request.DepartamentoSucursal.Trim(),
                    Latitud = 0,
                    Longitud = 0,
                    Telefono = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _sucursalService.CreateAsync(sucursal);
                
                if (string.IsNullOrEmpty(sucursal.Id))
                {
                    _logger.LogWarning("Sucursal creada pero sin ID, continuando sin sucursal");
                }
                else
                {
                    sucursalId = sucursal.Id;
                    _logger.LogInformation("Sucursal creada exitosamente con ID: {SucursalId}", sucursalId);
                }
            }

            // 5. CREAR USUARIO EMPRENDEDOR
            _logger.LogInformation("Creando usuario EMPRENDEDOR: {NombreUsuario} ({Email})", 
                request.NombreUsuario, request.Email);

            var usuario = new Usuario
            {
                EmpresaId = empresa.Id,
                SucursalId = sucursalId,
                TrabajadorId = null,
                Nombre = request.NombreUsuario.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Password = request.Password,
                TipoUsuario = "EMPRENDEDOR",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _usuarioService.CreateAsync(usuario);

            if (string.IsNullOrEmpty(usuario.Id))
            {
                _logger.LogError("Error crítico: El usuario se creó pero no se generó el ID");
                return StatusCode(500, new { 
                    error = "Error al crear el usuario", 
                    message = "No se pudo generar el ID del usuario. La empresa fue creada pero el usuario no." 
                });
            }

            _logger.LogInformation("Usuario EMPRENDEDOR creado exitosamente con ID: {UsuarioId}", usuario.Id);

            // 6. RESPUESTA EXITOSA
            return Ok(new
            {
                message = "Registro exitoso",
                empresa = new
                {
                    id = empresa.Id,
                    nombre = empresa.Nombre,
                    rubro = empresa.Rubro
                },
                sucursal = sucursalId != null ? new { id = sucursalId } : null,
                usuario = new
                {
                    id = usuario.Id,
                    nombre = usuario.Nombre,
                    email = usuario.Email,
                    tipoUsuario = usuario.TipoUsuario,
                    empresaId = usuario.EmpresaId,
                    sucursalId = usuario.SucursalId
                }
            });
        }
        catch (MongoWriteException mongoWriteEx)
        {
            _logger.LogError(mongoWriteEx, "Error de escritura en MongoDB");
            
            // Manejar errores de duplicados o índices únicos
            if (mongoWriteEx.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                return Conflict(new { 
                    error = "Registro duplicado", 
                    message = "Ya existe un registro con estos datos. Verifique el email o intente con otros datos." 
                });
            }
            
            return StatusCode(500, new { 
                error = "Error de base de datos", 
                message = "No se pudo guardar la información en la base de datos. Por favor, intente nuevamente." 
            });
        }
        catch (MongoException mongoEx)
        {
            _logger.LogError(mongoEx, "Error de conexión a MongoDB");
            return StatusCode(500, new { 
                error = "Error de conexión a la base de datos", 
                message = "No se pudo conectar a la base de datos. Verifique la configuración del servidor." 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inesperado en registro-emprendedor");
            return StatusCode(500, new { 
                error = "Error interno del servidor", 
                message = "Ocurrió un error inesperado. Por favor, intente nuevamente más tarde." 
            });
        }
    }

    [HttpPost("crear-trabajador")]
    [Authorize(Roles = "EMPRENDEDOR")]
    public async Task<ActionResult<object>> CrearTrabajador([FromBody] CrearTrabajadorRequest request)
    {
        // Obtener el EmpresaId del usuario logueado (EMPRENDEDOR)
        var empresaIdClaim = User.FindFirst("empresaId")?.Value;
        if (string.IsNullOrEmpty(empresaIdClaim))
            return Unauthorized("No se pudo obtener la empresa del usuario.");

        // Verificar si el email ya existe
        var usuarioExistente = await _usuarioService.GetByEmailAsync(request.Email);
        if (usuarioExistente is not null)
            return BadRequest("El email ya está registrado.");

        // Validar SucursalId si se proporciona
        if (!string.IsNullOrEmpty(request.SucursalId))
        {
            var sucursal = await _sucursalService.GetByIdAsync(request.SucursalId);
            if (sucursal is null)
                return BadRequest("La sucursal especificada no existe.");
            
            if (sucursal.EmpresaId != empresaIdClaim)
                return BadRequest("La sucursal no pertenece a tu empresa.");
        }

        // Crear el usuario TRABAJADOR
        var usuario = new Usuario
        {
            EmpresaId = empresaIdClaim,
            SucursalId = request.SucursalId,
            TrabajadorId = request.TrabajadorId,
            Nombre = request.Nombre,
            Email = request.Email,
            Password = request.Password,
            TipoUsuario = "TRABAJADOR",
            Activo = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _usuarioService.CreateAsync(usuario);

        return Ok(new
        {
            usuario = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Email,
                usuario.TipoUsuario,
                usuario.EmpresaId,
                usuario.SucursalId,
                usuario.TrabajadorId
            }
        });
    }
}

