using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public class ReporteService : IReporteService
{
    private readonly IReporteRepository _reportes;

    public ReporteService(IReporteRepository reportes)
    {
        _reportes = reportes;
    }

    public Task<IReadOnlyList<ReportePaisDto>> ObtenerPorPaisAsync(DateTime? desde, DateTime? hasta)
    {
        // Si las fechas vienen invertidas, se intercambian.
        if (desde.HasValue && hasta.HasValue && desde > hasta)
            (desde, hasta) = (hasta, desde);

        return _reportes.ObtenerPorPaisAsync(desde, hasta);
    }
}
