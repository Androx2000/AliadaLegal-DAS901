using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
}
