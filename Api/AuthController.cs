using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiBizly.Models;
using ApiBizly.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApiBizly.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(UsuarioService usuarioService, IOptions<JwtSettings> jwtOptions)
    {
        _usuarioService = usuarioService;
        _jwtSettings = jwtOptions.Value;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Contrasena { get; set; } = null!;
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        var usuario = await _usuarioService.GetByEmailAndPasswordAsync(
            request.Email, request.Contrasena);

        if (usuario is null)
            return Unauthorized("Credenciales inválidas.");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id!),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("rol", usuario.Rol),
            new Claim("empresaId", usuario.EmpresaId)
        };

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
                usuario.Rol,
                usuario.EmpresaId
            }
        });
    }
}
