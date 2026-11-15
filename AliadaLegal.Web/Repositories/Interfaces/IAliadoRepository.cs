using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Repositories;

public interface IAliadoRepository : IRepository<Aliado>

{
    Task<IReadOnlyList<Aliado>> ListarAsync(bool soloActivos);
    Task<IReadOnlyList<Aliado>> ListarPorPaisAsync(int paisId);
    
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId);
        Task<int> ContarTramitesAbiertosAsync(int aliadoId);
    
}
