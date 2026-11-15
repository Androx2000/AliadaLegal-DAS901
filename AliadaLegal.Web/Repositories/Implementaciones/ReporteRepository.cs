using AliadaLegal.Web.Data;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

public class ReporteRepository : IReporteRepository
{
    private readonly AppDbContext _db;

    public ReporteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ReportePaisDto>> ObtenerPorPaisAsync(DateTime? desde, DateTime? hasta)
    {
        var hastaExclusivo = hasta?.Date.AddDays(1);

        var tramites = _db.Tramites.AsNoTracking()
            .Where(t => desde == null || t.FechaSolicitud >= desde)
            .Where(t => hastaExclusivo == null || t.FechaSolicitud < hastaExclusivo);

        // 1) Conteos por país: GROUP BY resuelto en SQL.
        var conteos = await tramites
            .GroupBy(t => t.PaisDestino.Nombre)
            .Select(g => new
            {
                Pais = g.Key,
                Total = g.Count(),
                Entregados = g.Count(t => t.Estado == EstadoTramite.Entregado)
            })
            .ToListAsync();

        // 2) Fecha real de entrega: último cambio a Entregado en el historial.
        var entregas = await tramites
            .Where(t => t.Estado == EstadoTramite.Entregado)
            .Select(t => new
            {
                Pais = t.PaisDestino.Nombre,
                t.FechaSolicitud,
                Entrega = t.Historial
                    .Where(h => h.EstadoNuevo == EstadoTramite.Entregado)
                    .Max(h => (DateTime?)h.Fecha)
            })
            .ToListAsync();

        // 3) Promedio de días por país, en memoria (LINQ to Objects).
        var promedios = entregas
            .Where(e => e.Entrega != null)
            .GroupBy(e => e.Pais)
            .ToDictionary(
                g => g.Key,
                g => g.Average(e => (e.Entrega!.Value - e.FechaSolicitud).TotalDays));

        return conteos
            .OrderByDescending(c => c.Total)
            .ThenBy(c => c.Pais)
            .Select(c => new ReportePaisDto
            {
                Pais = c.Pais,
                Total = c.Total,
                Entregados = c.Entregados,
                Activos = c.Total - c.Entregados,
                DiasPromedioEntrega = promedios.TryGetValue(c.Pais, out var dias) ? Math.Round(dias, 1) : null
            })
            .ToList();
    }
}
