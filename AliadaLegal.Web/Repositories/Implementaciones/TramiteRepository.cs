using AliadaLegal.Web.Data;
using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

public class TramiteRepository : ITramiteRepository
{
    private readonly AppDbContext _db;

    public TramiteRepository(AppDbContext db)
    {
        _db = db;
    }

    private IQueryable<Tramite> ConRelaciones() =>
        _db.Tramites
            .Include(t => t.Cliente)
            .Include(t => t.TipoDocumento)
            .Include(t => t.PaisDestino)
            .Include(t => t.Aliado);

    public async Task<IReadOnlyList<Tramite>> BuscarAsync(TramiteFiltro filtro, int? limite = null)
    {
        var consulta = ConRelaciones().AsNoTracking();

        if (filtro.AliadoId.HasValue)
            consulta = consulta.Where(t => t.AliadoId == filtro.AliadoId.Value);

        if (filtro.ClienteId.HasValue)
            consulta = consulta.Where(t => t.ClienteId == filtro.ClienteId.Value);

        if (filtro.Estado.HasValue)
            consulta = consulta.Where(t => t.Estado == filtro.Estado.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            consulta = consulta.Where(t =>
                t.CodigoSeguimiento.Contains(texto) ||
                t.Cliente!.Nombre.Contains(texto));
        }

        consulta = consulta.OrderByDescending(t => t.FechaUltimaActualizacion);

        if (limite.HasValue)
            consulta = consulta.Take(limite.Value);

        return await consulta.ToListAsync();
    }

    public Task<Tramite?> ObtenerPorIdAsync(int id) =>
        ConRelaciones()
            .Include(t => t.Envio)
            .Include(t => t.Historial)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == id);

    public Task<Tramite?> ObtenerPorCodigoAsync(string codigo) =>
        ConRelaciones()
            .Include(t => t.Envio)
            .Include(t => t.Historial)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.CodigoSeguimiento == codigo);

    public Task<bool> CodigoExisteAsync(string codigo) =>
        _db.Tramites.AnyAsync(t => t.CodigoSeguimiento == codigo);

    public async Task<Dictionary<EstadoTramite, int>> ContarPorEstadoAsync(int? aliadoId)
    {
        var consulta = _db.Tramites.AsNoTracking().AsQueryable();
        if (aliadoId.HasValue)
            consulta = consulta.Where(t => t.AliadoId == aliadoId.Value);

        var filas = await consulta
            .GroupBy(t => t.Estado)
            .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        return filas.ToDictionary(f => f.Estado, f => f.Cantidad);
    }

    public async Task AgregarAsync(Tramite tramite) => await _db.Tramites.AddAsync(tramite);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
