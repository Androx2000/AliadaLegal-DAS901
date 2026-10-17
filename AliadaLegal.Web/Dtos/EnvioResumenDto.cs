namespace AliadaLegal.Web.Dtos;

public class EnvioResumenDto
{
    public int Id { get; set; }

    public int TramiteId { get; set; }

    public string EmpresaEnvio { get; set; }
        = string.Empty;

    public string NumeroGuia { get; set; }
        = string.Empty;

    public DateTime FechaDespacho { get; set; }

    public DateTime? FechaEntrega { get; set; }
}