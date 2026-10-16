using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public class AliadoService : IAliadoService
{
    private readonly IAliadoRepository _aliados;
    private readonly ICatalogoRepository _catalogos;

    public AliadoService(IAliadoRepository aliados, ICatalogoRepository catalogos)
    {
        _aliados = aliados;
        _catalogos = catalogos;
    }

    public Task<IReadOnlyList<Aliado>> ListarAsync(bool soloActivos) => _aliados.ListarAsync(soloActivos);

    public Task<IReadOnlyList<Aliado>> ListarPorPaisAsync(int paisId) => _aliados.ListarPorPaisAsync(paisId);

    public Task<Aliado?> ObtenerAsync(int id) => _aliados.ObtenerPorIdAsync(id);

    public async Task<ResultadoOperacion<Aliado>> CrearAsync(Aliado aliado)
    {
        aliado.Correo = aliado.Correo.Trim().ToLowerInvariant();
        aliado.Nombre = aliado.Nombre.Trim();

        if (await _catalogos.ObtenerPaisAsync(aliado.PaisAsignadoId) == null)
            return ResultadoOperacion<Aliado>.Fallo("El país asignado no existe.");

        if (await _aliados.ExisteCorreoAsync(aliado.Correo, null))
            return ResultadoOperacion<Aliado>.Fallo("Ya existe un aliado registrado con ese correo electrónico.");

        await _aliados.AgregarAsync(aliado);
        await _aliados.GuardarCambiosAsync();
        return ResultadoOperacion<Aliado>.Ok(aliado);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Aliado datos)
    {
        var existente = await _aliados.ObtenerPorIdAsync(datos.Id);
        if (existente == null)
            return ResultadoOperacion.Fallo("El aliado no existe.");

        var correo = datos.Correo.Trim().ToLowerInvariant();
        if (await _aliados.ExisteCorreoAsync(correo, datos.Id))
            return ResultadoOperacion.Fallo("Otro aliado ya usa ese correo electrónico.");

        var desactiva = existente.Activo && !datos.Activo;
        var cambiaPais = existente.PaisAsignadoId != datos.PaisAsignadoId;

        if (cambiaPais && await _catalogos.ObtenerPaisAsync(datos.PaisAsignadoId) == null)
            return ResultadoOperacion.Fallo("El país asignado no existe.");

        if (desactiva || cambiaPais)
        {
            var abiertos = await _aliados.ContarTramitesAbiertosAsync(existente.Id);
            if (abiertos > 0)
            {
                var accion = desactiva ? "desactivar al aliado" : "cambiar su país asignado";
                return ResultadoOperacion.Fallo(
                    $"No se puede {accion}: tiene {abiertos} trámite(s) abierto(s). Reasígnalos o espera a que se entreguen.");
            }
        }

        existente.Nombre = datos.Nombre.Trim();
        existente.Correo = correo;
        existente.Telefono = datos.Telefono.Trim();
        existente.TipoAliado = datos.TipoAliado;
        existente.PaisAsignadoId = datos.PaisAsignadoId;
        existente.Activo = datos.Activo;

        await _aliados.GuardarCambiosAsync();
        return ResultadoOperacion.Ok();
    }
}