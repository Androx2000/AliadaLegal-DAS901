using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using AliadaLegal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers;

[Authorize(Roles = Roles.AdministradorOAsesor)]
public class ClientesController : Controller
{
    private readonly IClienteService _clientes;
    private readonly ICatalogoService _catalogos;

    public ClientesController(IClienteService clientes, ICatalogoService catalogos)
    {
        _clientes = clientes;
        _catalogos = catalogos;
    }

    // GET /Clientes?texto=maria
    public async Task<IActionResult> Index(string? texto)
    {
        var lista = await _clientes.ListarAsync(texto);
        var vm = new ClienteListaViewModel
        {
            Texto = texto,
            Clientes = lista.Select(c => c.AItem()).ToList()
        };
        return View(vm);
    }

    // GET /Clientes/Details/5  (RF07: historial de trámites por cliente)
    public async Task<IActionResult> Details(int id)
    {
        var cliente = await _clientes.ObtenerAsync(id);
        if (cliente == null) return NotFound();

        return View(cliente.ADetalle());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new ClienteFormViewModel();
        await CargarPaisesAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClienteFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        var resultado = await _clientes.CrearAsync(vm.AEntidad());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        TempData["Exito"] = "Cliente registrado correctamente.";
        return RedirectToAction(nameof(Details), new { id = resultado.Valor!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _clientes.ObtenerAsync(id);
        if (cliente == null) return NotFound();

        var vm = cliente.AForm();
        await CargarPaisesAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClienteFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        var resultado = await _clientes.ActualizarAsync(vm.AEntidad());
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            await CargarPaisesAsync(vm);
            return View(vm);
        }

        TempData["Exito"] = "Los datos del cliente se actualizaron.";
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    private async Task CargarPaisesAsync(ClienteFormViewModel vm)
    {
        var paises = await _catalogos.ListarPaisesAsync();
        vm.Paises = paises.APaisesSelect(vm.PaisOrigenId);
    }
}
