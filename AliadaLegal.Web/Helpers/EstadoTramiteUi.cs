using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Helpers;

/// <summary>
/// Datos de presentación de cada estado del trámite: texto, clase CSS e ícono.
/// Lo usan el TagHelper &lt;estado-badge&gt; y las vistas (filtros, línea de tiempo).
/// Los colores están en wwwroot/css/site.css (sección 7) y en docs/manual-estilos.md.
/// </summary>
public static class EstadoTramiteUi
{
    public static string Etiqueta(EstadoTramite estado) => estado switch
    {
        EstadoTramite.Solicitado => "Solicitado",
        EstadoTramite.EnGestion => "En gestión",
        EstadoTramite.DocumentoObtenido => "Documento obtenido",
        EstadoTramite.Apostillado => "Apostillado",
        EstadoTramite.EnTransito => "En tránsito",
        EstadoTramite.Entregado => "Entregado",
        _ => estado.ToString()
    };

    /// <summary>Clase CSS: estado-solicitado, estado-engestion, …</summary>
    public static string ClaseCss(EstadoTramite estado) => "estado-" + estado.ToString().ToLowerInvariant();

    /// <summary>Clase CSS a partir de la clave en texto (por ejemplo "entransito" o "EnTransito").</summary>
    public static string ClaseCss(string? clave) =>
        "estado-" + (clave ?? string.Empty).Replace("estado-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();

    /// <summary>Ícono de Bootstrap Icons para cada estado.</summary>
    public static string Icono(EstadoTramite estado) => estado switch
    {
        EstadoTramite.Solicitado => "bi-inbox",
        EstadoTramite.EnGestion => "bi-hourglass-split",
        EstadoTramite.DocumentoObtenido => "bi-file-earmark-check",
        EstadoTramite.Apostillado => "bi-patch-check",
        EstadoTramite.EnTransito => "bi-truck",
        EstadoTramite.Entregado => "bi-check-circle",
        _ => "bi-circle"
    };

    /// <summary>Ícono a partir de la clave en texto (la usa la consulta pública, que recibe strings).</summary>
    public static string Icono(string? clave)
    {
        var limpia = (clave ?? string.Empty).Replace("estado-", string.Empty).Replace(" ", string.Empty);
        return Enum.TryParse<EstadoTramite>(limpia, ignoreCase: true, out var estado) ? Icono(estado) : "bi-circle";
    }

    /// <summary>Lista para el filtro de estados en Tramites/Index.</summary>
    public static IEnumerable<(EstadoTramite Valor, string Texto)> Todos() =>
        Enum.GetValues<EstadoTramite>().Select(e => (e, Etiqueta(e)));
}
