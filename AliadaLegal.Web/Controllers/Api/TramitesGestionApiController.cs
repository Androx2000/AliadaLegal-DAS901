using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers.Api;

/// <summary>
/// API REST de gestión de trámites (requiere sesión). Administrador y Asesor ven y gestionan todo;
/// un usuario Aliado solo ve sus trámites y solo puede avanzar su estado.
/// La consulta pública por código sigue en <see cref="TramitesApiController"/>.
/// </summary>
[ApiController]
[Route("api/tramites")]
[Produces("application/json")]
[Authorize]
public class TramitesGestionApiController : ControllerBase
{
    private readonly ITramiteService _tramites;
    private readonly ILogger<TramitesGestionApiController> _logger;

    public TramitesGestionApiController(ITramiteService tramites, ILogger<TramitesGestionApiController> logger)
    {
        _tramites = tramites;
        _logger = logger;
    }

    /// <summary>Si el usuario es Aliado, Id de su ficha; null para Administrador y Asesor.</summary>
    private int? AliadoDelUsuario => User.ObtenerAliadoId();

    /// <summary>Lista trámites con filtros opcionales. Un Aliado solo recibe los suyos.</summary>
    /// <param name="texto">Busca en el código de seguimiento o el nombre del cliente.</param>
    /// <param name="estado">Filtra por estado (1 a 6).</param>
    /// <param name="clienteId">Filtra por cliente.</param>
    /// <param name="aliadoId">Filtra por aliado (se ignora si el usuario es Aliado).</param>
    /// <response code="200">Lista de trámites, del más reciente al más antiguo.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TramiteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TramiteDto>>> Listar([FromQuery] string? texto,
        [FromQuery] EstadoTramite? estado, [FromQuery] int? clienteId, [FromQuery] int? aliadoId)
    {
        var filtro = new TramiteFiltro
        {
            Texto = texto,
            Estado = estado,
            ClienteId = clienteId,
            AliadoId = AliadoDelUsuario ?? aliadoId
        };

        var tramites = await _tramites.ListarAsync(filtro);
        return Ok(tramites.Select(t => t.ADto()));
    }

    /// <summary>Detalle de un trámite con su envío e historial de estados.</summary>
    /// <response code="200">El trámite.</response>
    /// <response code="404">No existe, o el usuario Aliado no lo tiene asignado.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TramiteDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TramiteDetalleDto>> Obtener(int id)
    {
        var tramite = await _tramites.ObtenerAsync(id, AliadoDelUsuario);
        if (tramite == null)
            return NoEncontrado(id);

        return Ok(tramite.ADetalleDto());
    }

    /// <summary>Registra una solicitud nueva (queda en estado Solicitado con su código de seguimiento).</summary>
    /// <response code="201">Trámite creado; Location apunta a GET /api/tramites/{id}.</response>
    /// <response code="400">Datos inválidos o cliente, tipo de documento o país inexistente.</response>
    [HttpPost]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    [ProducesResponseType(typeof(TramiteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TramiteDto>> Crear([FromBody] TramiteGuardarDto dto)
    {
        var resultado = await _tramites.CrearAsync(dto.ClienteId, dto.TipoDocumentoId, dto.PaisDestinoId,
            dto.Observaciones, User.ObtenerNombre());
        if (!resultado.Exito)
            return Rechazo("crear el trámite", null, resultado.Error, StatusCodes.Status400BadRequest);

        // Se vuelve a leer para devolver los nombres de cliente, país y documento.
        var creado = await _tramites.ObtenerAsync(resultado.Valor!.Id, null) ?? resultado.Valor!;
        _logger.LogInformation("Trámite {TramiteId} ({Codigo}) creado por {Usuario}",
            creado.Id, creado.CodigoSeguimiento, User.ObtenerNombre());
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado.ADto());
    }

    /// <summary>Edita un trámite. Solo se permite mientras está en estado Solicitado.</summary>
    /// <response code="204">Trámite actualizado.</response>
    /// <response code="400">Datos inválidos, el trámite ya avanzó de estado o un catálogo no existe (ver "detail").</response>
    /// <response code="404">No existe el trámite.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] TramiteGuardarDto dto)
    {
        if (await _tramites.ObtenerAsync(id, null) == null)
            return NoEncontrado(id);

        var resultado = await _tramites.ActualizarAsync(id, dto.ClienteId, dto.TipoDocumentoId,
            dto.PaisDestinoId, dto.Observaciones);
        if (!resultado.Exito)
            return Rechazo("actualizar el trámite", id, resultado.Error, StatusCodes.Status400BadRequest);

        _logger.LogInformation("Trámite {TramiteId} actualizado por {Usuario}", id, User.ObtenerNombre());
        return NoContent();
    }

    /// <summary>Elimina un trámite. Solo si sigue en Solicitado y sin aliado asignado.</summary>
    /// <response code="204">Trámite eliminado.</response>
    /// <response code="404">No existe el trámite.</response>
    /// <response code="409">El trámite ya tiene gestión registrada y se conserva.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (await _tramites.ObtenerAsync(id, null) == null)
            return NoEncontrado(id);

        var resultado = await _tramites.EliminarAsync(id);
        if (!resultado.Exito)
            return Rechazo("eliminar el trámite", id, resultado.Error, StatusCodes.Status409Conflict);

        _logger.LogInformation("Trámite {TramiteId} eliminado por {Usuario}", id, User.ObtenerNombre());
        return NoContent();
    }

    /// <summary>Asigna (o reasigna) el aliado que gestiona el trámite. Si estaba en Solicitado, pasa a En gestión.</summary>
    /// <response code="204">Aliado asignado.</response>
    /// <response code="400">Aliado inexistente, inactivo, de otro país, o trámite ya entregado.</response>
    /// <response code="404">No existe el trámite.</response>
    [HttpPut("{id:int}/aliado")]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AsignarAliado(int id, [FromBody] AsignarAliadoDto dto)
    {
        if (await _tramites.ObtenerAsync(id, null) == null)
            return NoEncontrado(id);

        var resultado = await _tramites.AsignarAliadoAsync(id, dto.AliadoId, User.ObtenerNombre());
        if (!resultado.Exito)
            return Rechazo("asignar el aliado", id, resultado.Error, StatusCodes.Status400BadRequest);

        _logger.LogInformation("Trámite {TramiteId} asignado al aliado {AliadoId} por {Usuario}",
            id, dto.AliadoId, User.ObtenerNombre());
        return NoContent();
    }

    /// <summary>
    /// Avanza el trámite al siguiente estado y lo registra en el historial.
    /// Un Aliado solo puede avanzar sus trámites y no puede registrar el envío ni la entrega.
    /// </summary>
    /// <response code="204">Estado actualizado.</response>
    /// <response code="400">Transición no permitida (los estados avanzan en orden).</response>
    /// <response code="404">No existe el trámite, o el Aliado no lo tiene asignado.</response>
    [HttpPatch("{id:int}/estado")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoDto dto)
    {
        // A un Aliado no se le revela si existe un trámite que no es suyo: responde 404.
        if (await _tramites.ObtenerAsync(id, AliadoDelUsuario) == null)
            return NoEncontrado(id);

        var resultado = await _tramites.CambiarEstadoAsync(id, dto.NuevoEstado, dto.Comentario,
            User.ObtenerNombre(), AliadoDelUsuario);
        if (!resultado.Exito)
            return Rechazo("cambiar el estado", id, resultado.Error, StatusCodes.Status400BadRequest);

        _logger.LogInformation("Trámite {TramiteId} pasó a {Estado} por {Usuario}",
            id, dto.NuevoEstado, User.ObtenerNombre());
        return NoContent();
    }

    /// <summary>Registra o corrige el envío del documento. El trámite pasa a En tránsito.</summary>
    /// <response code="204">Envío registrado.</response>
    /// <response code="400">Datos inválidos o el documento aún no está listo para enviarse.</response>
    /// <response code="404">No existe el trámite.</response>
    [HttpPut("{id:int}/envio")]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegistrarEnvio(int id, [FromBody] RegistrarEnvioDto dto)
    {
        if (await _tramites.ObtenerAsync(id, null) == null)
            return NoEncontrado(id);

        var resultado = await _tramites.RegistrarEnvioAsync(id, dto.NumeroGuia, dto.EmpresaEnvio,
            dto.FechaDespacho!.Value, User.ObtenerNombre());
        if (!resultado.Exito)
            return Rechazo("registrar el envío", id, resultado.Error, StatusCodes.Status400BadRequest);

        _logger.LogInformation("Envío del trámite {TramiteId} registrado por {Usuario} (guía {Guia})",
            id, User.ObtenerNombre(), dto.NumeroGuia);
        return NoContent();
    }

    private ObjectResult NoEncontrado(int id) =>
        Problem(detail: $"No existe un trámite con Id {id}.", statusCode: StatusCodes.Status404NotFound,
            title: "Trámite no encontrado.");

    /// <summary>Registra el rechazo de una regla de negocio y lo devuelve como ProblemDetails.</summary>
    private ObjectResult Rechazo(string accion, int? id, string? motivo, int codigo)
    {
        _logger.LogWarning("No se pudo {Accion} (trámite {TramiteId}): {Motivo}", accion, id, motivo);
        return Problem(detail: motivo, statusCode: codigo, title: $"No se pudo {accion}.");
    }
}
