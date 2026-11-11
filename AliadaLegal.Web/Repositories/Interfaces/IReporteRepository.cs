using AliadaLegal.Web.Dtos;

namespace AliadaLegal.Web.Repositories;

public interface IReporteRepository
{
    Task<IReadOnlyList<ReportePaisDto>> ObtenerPorPaisAsync(DateTime? desde, DateTime? hasta);
}
