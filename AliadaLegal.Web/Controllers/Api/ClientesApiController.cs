using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers.Api;

/// <summary>
/// API REST de clientes (CRUD). Reutiliza IClienteService, así que aplica las mismas
/// reglas de negocio que la interfaz MVC; aquí solo se traduce a HTTP y DTOs.
/// </summary>
[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
[Authorize(Roles = Roles.AdministradorOAsesor)]
public class ClientesApiController : ControllerBase
{
    private readonly IClienteService _clientes;
    private readonly ILogger<ClientesApiController> _logger;

    public ClientesApiController(IClienteService clientes, ILogger<ClientesApiController> logger)
    {
        _clientes = clientes;
        _logger = logger;
    }

    /// <summary>Lista los clientes; filtra por nombre o correo si se envía <paramref name="texto"/>.</summary>
    /// <param name="texto">Texto a buscar (opcional).</param>
    /// <response code="200">Lista de clientes (puede venir vacía).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteDto>>> Listar([FromQuery] string? texto)
    {
        var clientes = await _clientes.ListarAsync(texto);
        return Ok(clientes.Select(c => c.ADto()));
    }

    /// <summary>Obtiene un cliente por su Id.</summary>
    /// <response code="200">El cliente.</response>
    /// <response code="404">No existe un cliente con ese Id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDto>> Obtener(int id)
    {
        var cliente = await _clientes.ObtenerAsync(id);
        if (cliente == null)
            return NoEncontrado(id);

        return Ok(cliente.ADto());
    }

    /// <summary>Registra un cliente nuevo.</summary>
    /// <response code="201">Cliente creado; el encabezado Location apunta a GET /api/clientes/{id}.</response>
    /// <response code="400">Datos inválidos, país inexistente o correo ya registrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteDto>> Crear([FromBody] ClienteGuardarDto dto)
    {
        var resultado = await _clientes.CrearAsync(dto.AEntidad());
        if (!resultado.Exito)
        {
            _logger.LogWarning("Creación de cliente rechazada: {Motivo}", resultado.Error);
            return Problem(detail: resultado.Error, statusCode: StatusCodes.Status400BadRequest,
                title: "No se pudo crear el cliente.");
        }

        var creado = resultado.Valor!;
        _logger.LogInformation("Cliente {ClienteId} creado por {Usuario}", creado.Id, User.Identity?.Name);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado.ADto());
    }

    /// <summary>Actualiza los datos de un cliente existente.</summary>
    /// <response code="204">Cliente actualizado.</response>
    /// <response code="400">Datos inválidos, país inexistente o correo usado por otro cliente.</response>
    /// <response code="404">No existe un cliente con ese Id.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ClienteGuardarDto dto)
    {
        if (await _clientes.ObtenerAsync(id) == null)
            return NoEncontrado(id);

        var resultado = await _clientes.ActualizarAsync(dto.AEntidad(id));
        if (!resultado.Exito)
        {
            _logger.LogWarning("Actualización del cliente {ClienteId} rechazada: {Motivo}", id, resultado.Error);
            return Problem(detail: resultado.Error, statusCode: StatusCodes.Status400BadRequest,
                title: "No se pudo actualizar el cliente.");
        }

        _logger.LogInformation("Cliente {ClienteId} actualizado por {Usuario}", id, User.Identity?.Name);
        return NoContent();
    }

    /// <summary>Elimina un cliente. No se permite si tiene trámites registrados.</summary>
    /// <response code="204">Cliente eliminado.</response>
    /// <response code="404">No existe un cliente con ese Id.</response>
    /// <response code="409">El cliente tiene trámites y se conserva su historial.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (await _clientes.ObtenerAsync(id) == null)
            return NoEncontrado(id);

        var resultado = await _clientes.EliminarAsync(id);
        if (!resultado.Exito)
        {
            _logger.LogWarning("Eliminación del cliente {ClienteId} rechazada: {Motivo}", id, resultado.Error);
            return Problem(detail: resultado.Error, statusCode: StatusCodes.Status409Conflict,
                title: "No se pudo eliminar el cliente.");
        }

        _logger.LogInformation("Cliente {ClienteId} eliminado por {Usuario}", id, User.Identity?.Name);
        return NoContent();
    }

    private ObjectResult NoEncontrado(int id) =>
        Problem(detail: $"No existe un cliente con Id {id}.", statusCode: StatusCodes.Status404NotFound,
            title: "Cliente no encontrado.");
}
