using AliadaLegal.Web.Dtos;

namespace AliadaLegal.Web.ViewModels;

public class ReportePaisViewModel
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public IReadOnlyList<ReportePaisDto> Filas { get; set; } = new List<ReportePaisDto>();

    public int Total => Filas.Sum(f => f.Total);
    public int Activos => Filas.Sum(f => f.Activos);
    public int Entregados => Filas.Sum(f => f.Entregados);
}
