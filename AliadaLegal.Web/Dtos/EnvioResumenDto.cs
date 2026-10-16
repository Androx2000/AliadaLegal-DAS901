namespace AliadaLegal.Web.Dtos;

public class EnvioResumenDto
{
    public int Id { get; set; }

    public int TramiteId { get; set; }

    public string EmpresaTransporte { get; set; }
        = string.Empty;

    public string NumeroRastreo { get; set; }
        = string.Empty;

    public DateTime FechaEnvio { get; set; }

    public DateTime? FechaEntrega { get; set; }
}