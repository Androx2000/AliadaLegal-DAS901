using System.Security.Claims;

namespace AliadaLegal.Web.Helpers;

/// <summary>Lee los datos del usuario autenticado desde sus claims.</summary>
public static class UsuarioActualExtensions
{
    public const string ClaimAliadoId = "AliadoId";

    public static int ObtenerId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var id) ? id : 0;
    }

    public static string ObtenerNombre(this ClaimsPrincipal usuario) =>
        usuario.FindFirstValue(ClaimTypes.Name) ?? "Sistema";

    /// <summary>Id de la ficha de aliado cuando el usuario tiene rol Aliado; null en otros roles.</summary>
    public static int? ObtenerAliadoId(this ClaimsPrincipal usuario)
    {
        var valor = usuario.FindFirstValue(ClaimAliadoId);
        return int.TryParse(valor, out var id) ? id : null;
    }
}
