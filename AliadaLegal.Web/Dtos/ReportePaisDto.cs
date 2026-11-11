namespace AliadaLegal.Web.Dtos;

public class ReportePaisDto
{
    public string Pais { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Entregados { get; set; }
    public double? DiasPromedioEntrega { get; set; }
}
