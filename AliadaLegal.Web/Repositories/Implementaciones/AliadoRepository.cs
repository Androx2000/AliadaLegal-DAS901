using AliadaLegal.Web.Data;
using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

public class AliadoRepository : IAliadoRepository
{
    private readonly AppDbContext _db;

    public AliadoRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Aliado>> ListarAsync(bool soloActivos)
    {
        var consulta = _db.Aliados.AsNoTracking().Include(a => a.PaisAsignado).AsQueryable();
        if (soloActivos)
            consulta = consulta.Where(a => a.Activo);

        return await consulta.OrderBy(a => a.PaisAsignado!.Nombre).ThenBy(a => a.Nombre).ToListAsync();
    }

    public async Task<IReadOnlyList<Aliado>> ListarPorPaisAsync(int paisId) =>
        await _db.Aliados
            .AsNoTracking()
            .Include(a => a.PaisAsignado)
            .Where(a => a.Activo && a.PaisAsignadoId == paisId)
            .OrderBy(a => a.Nombre)
            .ToListAsync();

    public Task<Aliado?> ObtenerPorIdAsync(int id) =>
        _db.Aliados.Include(a => a.PaisAsignado).FirstOrDefaultAsync(a => a.Id == id);

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId) =>
        _db.Aliados.AnyAsync(a => a.Correo == correo && (excluirId == null || a.Id != excluirId));

    public async Task AgregarAsync(Aliado aliado) => await _db.Aliados.AddAsync(aliado);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
