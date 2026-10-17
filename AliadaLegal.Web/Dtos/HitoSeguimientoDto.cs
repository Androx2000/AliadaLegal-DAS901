namespace AliadaLegal.Web.Dtos;

public class HitoSeguimientoDto
{
    public string Estado { get; set; } = string.Empty;

    public string EstadoClave { get; set; } = string.Empty;

    public DateTime? Fecha { get; set; }

    public bool Completado { get; set; }

    public bool EsActual { get; set; }
}