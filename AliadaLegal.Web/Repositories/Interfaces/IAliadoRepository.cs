using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Repositories;

public interface IAliadoRepository
{
    Task<IReadOnlyList<Aliado>> ListarAsync(bool soloActivos);
    Task<IReadOnlyList<Aliado>> ListarPorPaisAsync(int paisId);
    Task<Aliado?> ObtenerPorIdAsync(int id);
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId);
    Task AgregarAsync(Aliado aliado);
        Task<int> ContarTramitesAbiertosAsync(int aliadoId);
    Task GuardarCambiosAsync();
}
