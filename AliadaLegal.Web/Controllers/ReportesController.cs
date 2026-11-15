using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using AliadaLegal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers;

[Authorize(Roles = Roles.AdministradorOAsesor)]
public class ReportesController : Controller
{
    private readonly IReporteService _reportes;

    public ReportesController(IReporteService reportes)
    {
        _reportes = reportes;
    }

    // GET /Reportes?desde=2026-01-01&hasta=2026-12-31
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
    {
        var filas = await _reportes.ObtenerPorPaisAsync(desde, hasta);
        return View(new ReportePaisViewModel { Desde = desde, Hasta = hasta, Filas = filas });
    }
}
