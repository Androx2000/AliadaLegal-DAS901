using AliadaLegal.Web.Data;
using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;

    public UsuarioRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo) =>
        _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == correo);
}
