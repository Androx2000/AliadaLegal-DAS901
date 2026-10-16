using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Repositories;

public class TramiteFiltro
{
    public string? Texto { get; set; }
    public EstadoTramite? Estado { get; set; }
    public int? AliadoId { get; set; }
    public int? ClienteId { get; set; }
}

public interface ITramiteRepository
{
    Task<IReadOnlyList<Tramite>> BuscarAsync(TramiteFiltro filtro, int? limite = null);
    Task<Tramite?> ObtenerPorIdAsync(int id);
    Task<Tramite?> ObtenerPorCodigoAsync(string codigo);
    Task<bool> CodigoExisteAsync(string codigo);
    Task<Dictionary<EstadoTramite, int>> ContarPorEstadoAsync(int? aliadoId);
    Task AgregarAsync(Tramite tramite);
    Task GuardarCambiosAsync();
}
