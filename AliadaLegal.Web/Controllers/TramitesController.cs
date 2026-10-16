using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using AliadaLegal.Web.Services;
using AliadaLegal.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AliadaLegal.Web.Controllers;

[Authorize]
public class TramitesController : Controller
{
    private readonly ITramiteService _tramites;
    private readonly IClienteService _clientes;
    private readonly IAliadoService _aliados;
    private readonly ICatalogoService _catalogos;

    public TramitesController(
        ITramiteService tramites,
        IClienteService clientes,
        IAliadoService aliados,
        ICatalogoService catalogos)
    {
        _tramites = tramites;
        _clientes = clientes;
        _aliados = aliados;
        _catalogos = catalogos;
    }

    private bool EsPersonalInterno => User.IsInRole(Roles.Administrador) || User.IsInRole(Roles.Asesor);

    // GET /Tramites?texto=AL-2026&estado=EnGestion
    public async Task<IActionResult> Index(string? texto, EstadoTramite? estado)
    {
        var aliadoId = User.ObtenerAliadoId();
        var filtro = new TramiteFiltro { Texto = texto, Estado = estado, AliadoId = aliadoId };
        var lista = await _tramites.ListarAsync(filtro);

        var vm = new TramiteListaViewModel
        {
            Texto = texto,
            Estado = estado,
            EsAliado = aliadoId.HasValue,
            Tramites = lista.Select(t => t.AItem()).ToList()
        };
        return View(vm);
    }

    // GET /Tramites/Details/7
    public async Task<IActionResult> Details(int id)
    {
        var tramite = await _tramites.ObtenerAsync(id, User.ObtenerAliadoId());
        if (tramite == null) return NotFound();

        return View(await ConstruirDetalleAsync(tramite));
    }

    // ---------------------------------------------------------------- Crear (RF02)
    [HttpGet]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    public async Task<IActionResult> Create(int? clienteId)
    {
        var vm = new TramiteFormViewModel { ClienteId = clienteId };
        await CargarListasAsync(vm);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    public async Task<IActionResult> Create(TramiteFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await CargarListasAsync(vm);
            return View(vm);
        }

        var resultado = await _tramites.CrearAsync(
            vm.ClienteId!.Value, vm.TipoDocumentoId!.Value, vm.PaisDestinoId!.Value,
            vm.Observaciones, User.ObtenerNombre());

        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            await CargarListasAsync(vm);
            return View(vm);
        }

        TempData["Exito"] = $"Trámite creado. Código de seguimiento: {resultado.Valor!.CodigoSeguimiento}";
        return RedirectToAction(nameof(Details), new { id = resultado.Valor!.Id });
    }

    // ------------------------------------------------------- Asignar aliado (RF03)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    public async Task<IActionResult> AsignarAliado(AsignarAliadoViewModel vm)
    {
        if (!ModelState.IsValid)
            return VolverConError(vm.TramiteId);

        var resultado = await _tramites.AsignarAliadoAsync(vm.TramiteId, vm.AliadoId!.Value, User.ObtenerNombre());
        return VolverConResultado(vm.TramiteId, resultado, "Aliado asignado. La gestión quedó en curso.");
    }

    // ------------------------------------------------------- Cambiar estado (RF04)
    // Lo pueden usar el personal interno y el aliado dueño del trámite; el servicio valida ambos casos.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(CambiarEstadoViewModel vm)
    {
        if (!ModelState.IsValid)
            return VolverConError(vm.TramiteId);

        var resultado = await _tramites.CambiarEstadoAsync(
            vm.TramiteId, vm.NuevoEstado, vm.Comentario, User.ObtenerNombre(), User.ObtenerAliadoId());

        return VolverConResultado(vm.TramiteId, resultado, "Estado actualizado.");
    }

    // ------------------------------------------------------- Registrar envío (RF08)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = Roles.AdministradorOAsesor)]
    public async Task<IActionResult> RegistrarEnvio(EnvioFormViewModel vm)
    {
        if (!ModelState.IsValid)
            return VolverConError(vm.TramiteId);

        var resultado = await _tramites.RegistrarEnvioAsync(
            vm.TramiteId, vm.NumeroGuia, vm.EmpresaEnvio, vm.FechaDespacho!.Value, User.ObtenerNombre());

        return VolverConResultado(vm.TramiteId, resultado, "Envío registrado. El trámite quedó en tránsito.");
    }

    // ------------------------------------------------------------------- Privados
    private IActionResult VolverConError(int tramiteId)
    {
        var primerError = ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

        TempData["Error"] = primerError ?? "Revise los datos ingresados.";
        return RedirectToAction(nameof(Details), new { id = tramiteId });
    }

    private IActionResult VolverConResultado(int tramiteId, ResultadoOperacion resultado, string mensajeExito)
    {
        if (resultado.Exito)
            TempData["Exito"] = mensajeExito;
        else
            TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Details), new { id = tramiteId });
    }

    private async Task CargarListasAsync(TramiteFormViewModel vm)
    {
        var clientes = await _clientes.ListarAsync(null);
        var tipos = await _catalogos.ListarTiposDocumentoAsync();
        var paises = await _catalogos.ListarPaisesAsync();

        vm.Clientes = clientes.AClientesSelect(vm.ClienteId);
        vm.TiposDocumento = tipos.ATiposSelect(vm.TipoDocumentoId);
        vm.Paises = paises.APaisesSelect(vm.PaisDestinoId);
    }

    private async Task<TramiteDetalleViewModel> ConstruirDetalleAsync(Tramite t)
    {
        var siguiente = t.EstadoSiguiente();
        var esInterno = EsPersonalInterno;
        var esAliado = User.ObtenerAliadoId().HasValue;
        var requiereApostilla = t.TipoDocumento?.RequiereApostilla ?? false;

        var vm = new TramiteDetalleViewModel
        {
            Id = t.Id,
            Codigo = t.CodigoSeguimiento,
            Estado = t.Estado,
            Progreso = EstadoTramiteFlujo.Progreso(t.Estado, requiereApostilla),
            EnlacePublico = t.GenerarEnlacePublico(),
            ClienteId = t.ClienteId,
            Cliente = t.Cliente?.Nombre ?? string.Empty,
            ClienteCorreo = t.Cliente?.Correo ?? string.Empty,
            ClienteContacto = t.Cliente?.ObtenerDetallesContacto() ?? string.Empty,
            TipoDocumento = t.TipoDocumento?.Nombre ?? string.Empty,
            RequiereApostilla = requiereApostilla,
            PaisDestino = t.PaisDestino?.Nombre ?? string.Empty,
            Observaciones = t.Observaciones,
            FechaSolicitud = t.FechaSolicitud,
            UltimaActualizacion = t.FechaUltimaActualizacion,
            Aliado = t.Aliado?.Nombre,
            AliadoContacto = t.Aliado?.ObtenerDetallesContacto(),
            EnvioGuia = t.Envio?.NumeroGuia,
            EnvioEmpresa = t.Envio?.EmpresaEnvio,
            EnvioFecha = t.Envio?.FechaDespacho,
            Historial = t.Historial
                .OrderBy(h => h.Fecha)
                .Select(h => new HistorialItemViewModel
                {
                    Estado = h.EstadoNuevo,
                    Fecha = h.Fecha,
                    Responsable = h.Responsable,
                    Comentario = h.Comentario
                })
                .ToList(),
            EstadoSiguiente = siguiente,
            PuedeAsignarAliado = esInterno && t.Estado != EstadoTramite.Entregado,
            PuedeAvanzar = siguiente.HasValue
                           && siguiente != EstadoTramite.EnTransito
                           && t.AliadoId != null
                           && (esInterno || (esAliado && siguiente < EstadoTramite.EnTransito)),
            PuedeRegistrarEnvio = esInterno
                                  && (siguiente == EstadoTramite.EnTransito || t.Estado == EstadoTramite.EnTransito)
        };

        vm.CambiarEstado = new CambiarEstadoViewModel
        {
            TramiteId = t.Id,
            NuevoEstado = siguiente ?? t.Estado
        };

        vm.EnvioForm = new EnvioFormViewModel
        {
            TramiteId = t.Id,
            NumeroGuia = t.Envio?.NumeroGuia ?? string.Empty,
            EmpresaEnvio = t.Envio?.EmpresaEnvio ?? string.Empty,
            FechaDespacho = t.Envio?.FechaDespacho ?? DateTime.Today
        };

        vm.Asignar = new AsignarAliadoViewModel { TramiteId = t.Id, AliadoId = t.AliadoId };
        if (vm.PuedeAsignarAliado)
        {
            var disponibles = await _aliados.ListarPorPaisAsync(t.PaisDestinoId);
            vm.Asignar.Aliados = disponibles.AAliadosSelect();
        }

        return vm;
    }
}
