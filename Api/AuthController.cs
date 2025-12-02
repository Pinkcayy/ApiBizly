using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public AuthController(
        UsuarioService usuarioService,
        EmpresaService empresaService,
        SucursalService sucursalService,
        IOptions<JwtSettings> jwtOptions)
    {
        _usuarioService = usuarioService;
        _empresaService = empresaService;
        _sucursalService = sucursalService;
        _jwtSettings = jwtOptions.Value;
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
        public string LogoUrl { get; set; } = string.Empty;
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
    public async Task<ActionResult<object>> RegistroEmprendedor([FromBody] RegistroEmprendedorRequest request)
    {
        try
        {
            // Validar campos requeridos
            var camposFaltantes = new List<string>();
            
            if (string.IsNullOrEmpty(request.Email))
                camposFaltantes.Add("email");
            
            if (string.IsNullOrEmpty(request.Password))
                camposFaltantes.Add("password");
            
            if (string.IsNullOrEmpty(request.NombreEmpresa))
                camposFaltantes.Add("nombreEmpresa");
            
            if (string.IsNullOrEmpty(request.NombreUsuario))
                camposFaltantes.Add("nombreUsuario");
            
            if (camposFaltantes.Any())
            {
                return BadRequest(new { 
                    error = "Campos requeridos faltantes", 
                    camposFaltantes = camposFaltantes,
                    mensaje = $"Los siguientes campos son requeridos: {string.Join(", ", camposFaltantes)}"
                });
            }

            // Verificar si el email ya existe
            var usuarioExistente = await _usuarioService.GetByEmailAsync(request.Email);
            if (usuarioExistente is not null)
                return BadRequest("El email ya está registrado.");

            // Crear la empresa
            var empresa = new Empresa
            {
                Nombre = request.NombreEmpresa,
                Rubro = request.Rubro ?? string.Empty,
                Descripcion = request.DescripcionEmpresa ?? string.Empty,
                MargenGanancia = request.MargenGanancia,
                LogoUrl = request.LogoUrl ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _empresaService.CreateAsync(empresa);

            // Verificar que la empresa se creó correctamente
            if (string.IsNullOrEmpty(empresa.Id))
                return StatusCode(500, "Error al crear la empresa. No se generó el ID.");

            // Crear sucursal principal (opcional)
            string? sucursalId = null;
            if (!string.IsNullOrEmpty(request.NombreSucursal))
            {
                var sucursal = new Sucursal
                {
                    EmpresaId = empresa.Id,
                    Nombre = request.NombreSucursal,
                    Direccion = request.DireccionSucursal ?? string.Empty,
                    Ciudad = request.CiudadSucursal ?? string.Empty,
                    Departamento = request.DepartamentoSucursal ?? string.Empty,
                    Latitud = 0,
                    Longitud = 0,
                    Telefono = string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _sucursalService.CreateAsync(sucursal);
                sucursalId = sucursal.Id;
            }

            // Crear el usuario EMPRENDEDOR
            var usuario = new Usuario
            {
                EmpresaId = empresa.Id,
                SucursalId = sucursalId,
                TrabajadorId = null,
                Nombre = request.NombreUsuario,
                Email = request.Email,
                Password = request.Password,
                TipoUsuario = "EMPRENDEDOR",
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _usuarioService.CreateAsync(usuario);

            return Ok(new
            {
                empresa = new
                {
                    empresa.Id,
                    empresa.Nombre,
                    empresa.Rubro
                },
                sucursal = sucursalId != null ? new { Id = sucursalId } : null,
                usuario = new
                {
                    usuario.Id,
                    usuario.Nombre,
                    usuario.Email,
                    usuario.TipoUsuario,
                    usuario.EmpresaId,
                    usuario.SucursalId
                }
            });
        }
        catch (MongoException mongoEx)
        {
            return StatusCode(500, new { 
                error = "Error de conexión a MongoDB", 
                mensaje = "No se pudo conectar a la base de datos. Verifica la configuración de MongoDB.",
                detalle = mongoEx.Message
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                error = "Error interno del servidor", 
                mensaje = ex.Message,
                detalle = ex.InnerException?.Message
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
