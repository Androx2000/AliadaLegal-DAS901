using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Services;

public interface IJwtTokenService
{
    /// <summary>Genera un token JWT firmado para el usuario (ya validado) con su rol y, si aplica, su ficha de aliado.</summary>
    TokenResponseDto GenerarToken(Usuario usuario);
}
