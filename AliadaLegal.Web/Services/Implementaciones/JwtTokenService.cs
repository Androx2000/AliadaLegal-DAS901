using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Infraestructura;
using AliadaLegal.Web.Models;
using Microsoft.IdentityModel.Tokens;

namespace AliadaLegal.Web.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _opciones;
    private readonly TimeProvider _reloj;

    public JwtTokenService(JwtOptions opciones, TimeProvider reloj)
    {
        _opciones = opciones;
        _reloj = reloj;
    }

    public TokenResponseDto GenerarToken(Usuario usuario)
    {
        var ahora = _reloj.GetUtcNow().UtcDateTime;
        var expira = ahora.AddMinutes(_opciones.MinutosExpiracion);
        var rol = usuario.Rol?.NombreRol ?? string.Empty;

        // Mismos claims que la cookie, para que [Authorize(Roles = ...)] y los helpers funcionen igual.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, rol),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (usuario.AliadoId.HasValue)
            claims.Add(new Claim(UsuarioActualExtensions.ClaimAliadoId, usuario.AliadoId.Value.ToString()));

        var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key));
        var credenciales = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _opciones.Issuer,
            audience: _opciones.Audience,
            claims: claims,
            notBefore: ahora,
            expires: expira,
            signingCredentials: credenciales);

        return new TokenResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiraEn = expira,
            ExpiraEnSegundos = _opciones.MinutosExpiracion * 60,
            Usuario = new UsuarioActualDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = rol,
                AliadoId = usuario.AliadoId
            }
        };
    }
}
