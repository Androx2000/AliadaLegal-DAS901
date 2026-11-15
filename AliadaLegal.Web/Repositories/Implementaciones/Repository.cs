using AliadaLegal.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

/// <summary>
/// Implementación base del CRUD con Entity Framework Core.
/// Los métodos son virtuales para que un repositorio hijo los cambie si necesita Include u orden.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(AppDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public virtual async Task<T?> ObtenerPorIdAsync(int id) => await _set.FindAsync(id);

    public virtual async Task<IReadOnlyList<T>> ListarTodosAsync() =>
        await _set.AsNoTracking().ToListAsync();

    public virtual async Task AgregarAsync(T entidad) => await _set.AddAsync(entidad);

    public virtual void Actualizar(T entidad) => _set.Update(entidad);

    public virtual void Eliminar(T entidad) => _set.Remove(entidad);

    public Task GuardarCambiosAsync() => _db.SaveChangesAsync();
}