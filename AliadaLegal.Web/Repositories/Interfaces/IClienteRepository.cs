using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Repositories;

public interface IClienteRepository
{
    Task<IReadOnlyList<Cliente>> ListarAsync(string? texto);
    Task<Cliente?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId);
    Task AgregarAsync(Cliente cliente);
    Task GuardarCambiosAsync();
}
