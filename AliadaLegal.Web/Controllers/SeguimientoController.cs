using AliadaLegal.Web.Services;
using AliadaLegal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers;

/// <summary>Vista pública limitada (RF06): el cliente consulta su trámite solo con el código de seguimiento.</summary>
[AllowAnonymous]
[Route("seguimiento")]
public class SeguimientoController : Controller
{
    private readonly ITramiteService _tramites;

    public SeguimientoController(ITramiteService tramites)
    {
        _tramites = tramites;
    }

    [HttpGet("")]
    public IActionResult Index() => View(new ConsultaSeguimientoViewModel());

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public IActionResult Buscar(ConsultaSeguimientoViewModel vm)
    {
        if (!ModelState.IsValid)
            return View("Index", vm);

        // Patrón Post-Redirect-Get: la consulta queda en una URL que el cliente puede guardar o compartir.
        return RedirectToAction(nameof(Consultar), new { codigo = vm.Codigo.Trim().ToUpperInvariant() });
    }

    [HttpGet("{codigo}")]
    public async Task<IActionResult> Consultar(string codigo)
    {
        var resultado = await _tramites.ConsultarSeguimientoAsync(codigo);

        var vm = new ConsultaSeguimientoViewModel
        {
            Codigo = codigo,
            Buscado = true,
            Resultado = resultado
        };
        return View("Index", vm);
    }
}
