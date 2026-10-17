namespace AliadaLegal.Web.Models;

/// <summary>
/// Reglas de avance entre estados. Está aislada del resto del código para que
/// agregar o reordenar estados no obligue a modificar el flujo central (OCP).
/// </summary>
public static class EstadoTramiteFlujo
{
    private static readonly EstadoTramite[] Orden =
    {
        EstadoTramite.Solicitado,
        EstadoTramite.EnGestion,
        EstadoTramite.DocumentoObtenido,
        EstadoTramite.Apostillado,
        EstadoTramite.EnTransito,
        EstadoTramite.Entregado
    };

    /// <summary>Estados que el trámite recorrerá, según si el documento requiere apostilla.</summary>
    public static IReadOnlyList<EstadoTramite> Recorrido(bool requiereApostilla) =>
        Orden.Where(e => requiereApostilla || e != EstadoTramite.Apostillado).ToList();

    /// <summary>El único estado al que se puede avanzar desde el actual (null si ya terminó).</summary>
    public static EstadoTramite? Siguiente(EstadoTramite actual, bool requiereApostilla)
    {
        var recorrido = Recorrido(requiereApostilla);
        var indice = recorrido.ToList().IndexOf(actual);
        if (indice < 0 || indice >= recorrido.Count - 1) return null;
        return recorrido[indice + 1];
    }

    public static bool EsTransicionValida(EstadoTramite actual, EstadoTramite nuevo, bool requiereApostilla) =>
        Siguiente(actual, requiereApostilla) == nuevo;

    /// <summary>Porcentaje de avance (0 a 100) para la barra de progreso.</summary>
    public static int Progreso(EstadoTramite actual, bool requiereApostilla)
    {
        var recorrido = Recorrido(requiereApostilla).ToList();
        var indice = recorrido.IndexOf(actual);
        if (indice < 0 || recorrido.Count < 2) return 0;
        return (int)Math.Round(indice * 100.0 / (recorrido.Count - 1));
    }
}
