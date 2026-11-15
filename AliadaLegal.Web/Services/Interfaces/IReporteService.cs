using AliadaLegal.Web.Dtos;

namespace AliadaLegal.Web.Services;

public interface IReporteService
{
    Task<IReadOnlyList<ReportePaisDto>> ObtenerPorPaisAsync(DateTime? desde, DateTime? hasta);
}
