namespace AliadaLegal.Web.Dtos;

public class TramiteResumenDto
{
    public int Id { get; set; }

    public string NombreCliente { get; set; }
        = string.Empty;

    public string Pais { get; set; }
        = string.Empty;

    public string TipoDocumento { get; set; }
        = string.Empty;

    public string Estado { get; set; }
        = string.Empty;

    public string? NombreAliado { get; set; }

    public DateTime FechaSolicitud { get; set; }
}