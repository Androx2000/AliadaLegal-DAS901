using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers.Api;

/// <summary>API REST de solo lectura con los mismos datos limitados que la vista pública.</summary>
[ApiController]
[AllowAnonymous]
[Route("api/tramites")]
public class TramitesApiController : ControllerBase
{
    private readonly ISeguimientoService _seguimiento;

    public TramitesApiController(ISeguimientoService seguimiento)
    {
        _seguimiento = seguimiento;
    }

    /// <summary>Consulta pública del avance de un trámite por su código (no requiere sesión).</summary>
    /// <param name="codigo">Código de seguimiento, p. ej. AL-2026-K7M2QX.</param>
    /// <response code="200">Estado, progreso e hitos del trámite.</response>
    /// <response code="404">No existe un trámite con ese código.</response>
    [HttpGet("seguimiento/{codigo}")]
    [ProducesResponseType(typeof(TramiteSeguimientoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TramiteSeguimientoDto>> Seguimiento(string codigo)
    {
        var dto = await _seguimiento.ConsultarAsync(codigo);
        if (dto == null)
            return Problem(detail: "No se encontró un trámite con ese código.",
                statusCode: StatusCodes.Status404NotFound, title: "Trámite no encontrado.");

        return Ok(dto);
    }
}
