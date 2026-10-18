using AliadaLegal.Web.Data;
using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _db;

    public ClienteRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync(string? texto)
    {
        var consulta = _db.Clientes
            .AsNoTracking()
            .Include(c => c.PaisOrigen)
            .Include(c => c.Tramites)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            consulta = consulta.Where(c => c.Nombre.Contains(t) || c.Correo.Contains(t));
        }

        return await consulta.OrderBy(c => c.Nombre).ToListAsync();
    }

    public Task<Cliente?> ObtenerPorIdAsync(int id) =>
        _db.Clientes
            .Include(c => c.PaisOrigen)
            .Include(c => c.Tramites).ThenInclude(t => t.TipoDocumento)
            .Include(c => c.Tramites).ThenInclude(t => t.PaisDestino)
            .Include(c => c.Tramites).ThenInclude(t => t.Aliado)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId) =>
        _db.Clientes.AnyAsync(c => c.Correo == correo && (excluirId == null || c.Id != excluirId));

    public async Task AgregarAsync(Cliente cliente) => await _db.Clientes.AddAsync(cliente);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}
