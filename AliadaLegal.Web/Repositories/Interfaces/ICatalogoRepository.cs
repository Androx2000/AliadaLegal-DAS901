using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Repositories;

public interface ICatalogoRepository
{
    Task<IReadOnlyList<Pais>> ListarPaisesAsync();
    Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync();
    Task<Pais?> ObtenerPaisAsync(int id);
    Task<TipoDocumento?> ObtenerTipoDocumentoAsync(int id);
}
