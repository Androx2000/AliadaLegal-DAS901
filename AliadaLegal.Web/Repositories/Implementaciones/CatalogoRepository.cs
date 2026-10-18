using AliadaLegal.Web.Data;
using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

public class CatalogoRepository : ICatalogoRepository
{
    private readonly AppDbContext _db;

    public CatalogoRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Pais>> ListarPaisesAsync() =>
        await _db.Paises.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();

    public async Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync() =>
        await _db.TiposDocumento.AsNoTracking().OrderBy(t => t.Nombre).ToListAsync();

    public Task<Pais?> ObtenerPaisAsync(int id) => _db.Paises.FirstOrDefaultAsync(p => p.Id == id);

    public Task<TipoDocumento?> ObtenerTipoDocumentoAsync(int id) =>
        _db.TiposDocumento.FirstOrDefaultAsync(t => t.Id == id);
}
