using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Almacen.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace Almacen.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerarAccessToken(UsuarioMobile usuario)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key no configurado.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "almacen-mobile-api";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "almacen-mobile-app";
        var expiraMinutos = int.Parse(_configuration["Jwt:AccessTokenMinutos"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreCompleto),
            new(ClaimTypes.Email, usuario.Email),
        };

        if (usuario.RolId.HasValue)
            claims.Add(new Claim(ClaimTypes.Role, usuario.RolNombre ?? "USUARIO"));

        if (usuario.InstitucionId.HasValue)
            claims.Add(new Claim("institucion_id", usuario.InstitucionId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiraMinutos),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerarRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public DateTime ObtenerFechaExpiracion()
    {
        var expiraMinutos = int.Parse(_configuration["Jwt:AccessTokenMinutos"] ?? "60");
        return DateTime.UtcNow.AddMinutes(expiraMinutos);
    }
}