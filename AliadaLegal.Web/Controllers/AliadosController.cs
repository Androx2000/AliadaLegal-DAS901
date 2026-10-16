using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using AliadaLegal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers;

[Authorize(Roles = Roles.AdministradorOAsesor)]
public class AliadosController : Controller
{
    private readonly IAliadoService _aliados;
    private readonly ICatalogoService _catalogos;

    public AliadosController(IAliadoService aliados, ICatalogoService catalogos)
    {
        _aliados = aliados;
        _catalogos = catalogos;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _aliados.ListarAsync(soloActivos: false);
        return View(new AliadoListaViewModel { Aliados = lista.Select(a => a.AItem()).ToList() });
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Create()
    {
        var vm = new AliadoFormViewModel();
        await CargarPaisesAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Create(AliadoFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        var resultado = await _aliados.CrearAsync(vm.AEntidad());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        TempData["Exito"] = "Aliado registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Edit(int id)
    {
        var aliado = await _aliados.ObtenerAsync(id);
        if (aliado == null) return NotFound();

        var vm = aliado.AForm();
        await CargarPaisesAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Edit(AliadoFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        var resultado = await _aliados.ActualizarAsync(vm.AEntidad());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        TempData["Exito"] = "Los datos del aliado se actualizaron.";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarPaisesAsync(AliadoFormViewModel vm)
    {
        var paises = await _catalogos.ListarPaisesAsync();
        vm.Paises = paises.APaisesSelect(vm.PaisAsignadoId);
    }
}
