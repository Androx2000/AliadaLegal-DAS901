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
    private readonly ITramiteService _tramites;

    public TramitesApiController(ITramiteService tramites)
    {
        _tramites = tramites;
    }

    /// <summary>GET /api/tramites/seguimiento/AL-2026-K7M2QX</summary>
    [HttpGet("seguimiento/{codigo}")]
    [ProducesResponseType(typeof(TramiteSeguimientoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TramiteSeguimientoDto>> Seguimiento(string codigo)
    {
        var dto = await _tramites.ConsultarSeguimientoAsync(codigo);
        if (dto == null)
            return NotFound(new { mensaje = "No se encontró un trámite con ese código." });

        return Ok(dto);
    }
}
