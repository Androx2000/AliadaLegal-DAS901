using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public interface IAuthService
{
    /// <summary>Devuelve el usuario si las credenciales son correctas y está activo; si no, null.</summary>
    Task<Usuario?> ValidarCredencialesAsync(string correo, string password);
}

public interface ICatalogoService
{
    Task<IReadOnlyList<Pais>> ListarPaisesAsync();
    Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync();
}

public interface IClienteService
{
    Task<IReadOnlyList<Cliente>> ListarAsync(string? texto);
    Task<Cliente?> ObtenerAsync(int id);
    Task<ResultadoOperacion<Cliente>> CrearAsync(Cliente cliente);
    Task<ResultadoOperacion> ActualizarAsync(Cliente datos);
}

public interface IAliadoService
{
    Task<IReadOnlyList<Aliado>> ListarAsync(bool soloActivos);
    Task<IReadOnlyList<Aliado>> ListarPorPaisAsync(int paisId);
    Task<Aliado?> ObtenerAsync(int id);
    Task<ResultadoOperacion<Aliado>> CrearAsync(Aliado aliado);
    Task<ResultadoOperacion> ActualizarAsync(Aliado datos);
}

public interface ITramiteService
{
    Task<ResultadoOperacion<Tramite>> CrearAsync(int clienteId, int tipoDocumentoId, int paisDestinoId,
        string? observaciones, string responsable);

    Task<ResultadoOperacion> AsignarAliadoAsync(int tramiteId, int aliadoId, string responsable);

    /// <param name="aliadoIdRestriccion">Si viene con valor, solo se permite operar sobre trámites de ese aliado.</param>
    Task<ResultadoOperacion> CambiarEstadoAsync(int tramiteId, EstadoTramite nuevo, string? comentario,
        string responsable, int? aliadoIdRestriccion);

    Task<ResultadoOperacion> RegistrarEnvioAsync(int tramiteId, string numeroGuia, string empresaEnvio,
        DateTime fechaDespacho, string responsable);

    Task<IReadOnlyList<Tramite>> ListarAsync(TramiteFiltro filtro, int? limite = null);
    Task<Tramite?> ObtenerAsync(int id, int? aliadoIdRestriccion);
    Task<TramiteSeguimientoDto?> ConsultarSeguimientoAsync(string codigo);
    Task<ResumenTramitesDto> ObtenerResumenAsync(int? aliadoId);
}
