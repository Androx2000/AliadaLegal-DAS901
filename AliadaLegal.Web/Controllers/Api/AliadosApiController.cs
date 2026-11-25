using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers.Api;

/// <summary>
/// API REST de aliados (abogados y notarios). Consultar: Administrador o Asesor.
/// Crear, modificar y dar de baja: solo Administrador (igual que en la interfaz MVC).
/// </summary>
[ApiController]
[Route("api/aliados")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdministradorOAsesor)]
public class AliadosApiController : ControllerBase
{
    private readonly IAliadoService _aliados;
    private readonly ILogger<AliadosApiController> _logger;

    public AliadosApiController(IAliadoService aliados, ILogger<AliadosApiController> logger)
    {
        _aliados = aliados;
        _logger = logger;
    }

    /// <summary>Lista los aliados. Con <paramref name="paisId"/> devuelve solo los de ese país.</summary>
    /// <param name="soloActivos">true para excluir los dados de baja (por defecto false).</param>
    /// <param name="paisId">Filtra por país asignado (opcional).</param>
    /// <response code="200">Lista de aliados (puede venir vacía).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AliadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<AliadoDto>>> Listar(
        [FromQuery] bool soloActivos = false, [FromQuery] int? paisId = null)
    {
        var aliados = paisId.HasValue
            ? await _aliados.ListarPorPaisAsync(paisId.Value)
            : await _aliados.ListarAsync(soloActivos);

        return Ok(aliados.Where(a => !soloActivos || a.Activo).Select(a => a.ADto()));
    }

    /// <summary>Obtiene un aliado por su Id.</summary>
    /// <response code="200">El aliado.</response>
    /// <response code="404">No existe un aliado con ese Id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AliadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AliadoDto>> Obtener(int id)
    {
        var aliado = await _aliados.ObtenerAsync(id);
        if (aliado == null)
            return NoEncontrado(id);

        return Ok(aliado.ADto());
    }

    /// <summary>Registra un aliado nuevo.</summary>
    /// <response code="201">Aliado creado; Location apunta a GET /api/aliados/{id}.</response>
    /// <response code="400">Datos inválidos, país inexistente o correo ya registrado.</response>
    /// <response code="403">El usuario no es Administrador.</response>
    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(typeof(AliadoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AliadoDto>> Crear([FromBody] AliadoGuardarDto dto)
    {
        var resultado = await _aliados.CrearAsync(dto.AEntidad());
        if (!resultado.Exito)
        {
            _logger.LogWarning("Creación de aliado rechazada: {Motivo}", resultado.Error);
            return Problem(detail: resultado.Error, statusCode: StatusCodes.Status400BadRequest,
                title: "No se pudo crear el aliado.");
        }

        var creado = resultado.Valor!;
        _logger.LogInformation("Aliado {AliadoId} creado por {Usuario}", creado.Id, User.Identity?.Name);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado.ADto());
    }

    /// <summary>
    /// Actualiza un aliado. No se permite desactivarlo ni cambiar su país mientras tenga trámites abiertos.
    /// </summary>
    /// <response code="204">Aliado actualizado.</response>
    /// <response code="400">Datos inválidos o regla de negocio incumplida (ver "detail").</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">No existe un aliado con ese Id.</response>
    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] AliadoGuardarDto dto)
    {
        if (await _aliados.ObtenerAsync(id) == null)
            return NoEncontrado(id);

        var resultado = await _aliados.ActualizarAsync(dto.AEntidad(id));
        if (!resultado.Exito)
        {
            _logger.LogWarning("Actualización del aliado {AliadoId} rechazada: {Motivo}", id, resultado.Error);
            return Problem(detail: resultado.Error, statusCode: StatusCodes.Status400BadRequest,
                title: "No se pudo actualizar el aliado.");
        }

        _logger.LogInformation("Aliado {AliadoId} actualizado por {Usuario}", id, User.Identity?.Name);
        return NoContent();
    }

    /// <summary>
    /// Da de baja a un aliado (baja lógica: Activo = false). Los aliados no se borran para
    /// conservar el historial de los trámites que gestionaron.
    /// </summary>
    /// <response code="204">Aliado dado de baja (o ya estaba inactivo).</response>
    /// <response code="403">El usuario no es Administrador.</response>
    /// <response code="404">No existe un aliado con ese Id.</response>
    /// <response code="409">Tiene trámites abiertos: hay que reasignarlos primero.</response>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DarDeBaja(int id)
    {
        var aliado = await _aliados.ObtenerAsync(id);
        if (aliado == null)
            return NoEncontrado(id);

        if (!aliado.Activo)
            return NoContent(); // DELETE es idempotente: repetirlo no cambia nada.

        // Copia con Activo = false: el servicio compara contra la entidad guardada
        // y aplica la regla de los trámites abiertos.
        var baja = new Aliado
        {
            Id = aliado.Id, Nombre = aliado.Nombre, Correo = aliado.Correo, Telefono = aliado.Telefono,
            TipoAliado = aliado.TipoAliado, PaisAsignadoId = aliado.PaisAsignadoId, Activo = false
        };

        var resultado = await _aliados.ActualizarAsync(baja);
        if (!resultado.Exito)
        {
            _logger.LogWarning("Baja del aliado {AliadoId} rechazada: {Motivo}", id, resultado.Error);
            return Problem(detail: resultado.Error, statusCode: StatusCodes.Status409Conflict,
                title: "No se pudo dar de baja al aliado.");
        }

        _logger.LogInformation("Aliado {AliadoId} dado de baja por {Usuario}", id, User.Identity?.Name);
        return NoContent();
    }

    private ObjectResult NoEncontrado(int id) =>
        Problem(detail: $"No existe un aliado con Id {id}.", statusCode: StatusCodes.Status404NotFound,
            title: "Aliado no encontrado.");
}
