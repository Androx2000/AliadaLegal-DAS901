using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _catalogos;

    public CatalogoService(ICatalogoRepository catalogos)
    {
        _catalogos = catalogos;
    }

    public Task<IReadOnlyList<Pais>> ListarPaisesAsync() => _catalogos.ListarPaisesAsync();

    public Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync() =>
        _catalogos.ListarTiposDocumentoAsync();
}
