namespace AliadaLegal.Web.Models;

/// <summary>
/// Operaciones de dominio que usan los controladores sobre las entidades del modelo.
/// Se declaran como métodos de extensión para no modificar las clases de Models/.
/// </summary>
public static class ModeloExtensions
{
    /// <summary>Siguiente estado permitido; omite Apostillado si el tipo de documento no lo requiere.</summary>
    public static EstadoTramite? EstadoSiguiente(this Tramite t) =>
        EstadoTramiteFlujo.Siguiente(t.Estado, t.TipoDocumento?.RequiereApostilla ?? false);

    /// <summary>Ruta pública de consulta del trámite.</summary>
    public static string GenerarEnlacePublico(this Tramite t) => $"/seguimiento/{t.CodigoSeguimiento}";

    public static string ObtenerDetallesContacto(this Cliente c) => $"{c.Telefono} ({c.CanalContacto})";

    public static string ObtenerDetallesContacto(this Aliado a) => $"{a.TipoAliado} · {a.Telefono}";
}
