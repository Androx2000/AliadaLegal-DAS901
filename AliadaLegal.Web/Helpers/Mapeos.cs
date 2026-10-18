using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AliadaLegal.Web.Helpers;

/// <summary>
/// Conversión entre entidades y ViewModels. Las vistas nunca reciben entidades de EF directamente:
/// así no se exponen campos internos ni se habilita el over-posting al enlazar formularios.
/// </summary>
public static class Mapeos
{
    public static TramiteItemViewModel AItem(this Tramite t) => new()
    {
        Id = t.Id,
        Codigo = t.CodigoSeguimiento,
        ClienteId = t.ClienteId,
        Cliente = t.Cliente?.Nombre ?? string.Empty,
        TipoDocumento = t.TipoDocumento?.Nombre ?? string.Empty,
        PaisDestino = t.PaisDestino?.Nombre ?? string.Empty,
        Estado = t.Estado,
        Aliado = t.Aliado?.Nombre,
        FechaSolicitud = t.FechaSolicitud,
        UltimaActualizacion = t.FechaUltimaActualizacion
    };

    public static ClienteItemViewModel AItem(this Cliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Correo = c.Correo,
        Telefono = c.Telefono,
        TipoCliente = c.TipoCliente.NombreMostrable(),
        EsBufete = c.TipoCliente == TipoCliente.Bufete,
        PaisOrigen = c.PaisOrigen?.Nombre ?? string.Empty,
        TotalTramites = c.Tramites.Count
    };

    public static AliadoItemViewModel AItem(this Aliado a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Correo = a.Correo,
        Telefono = a.Telefono,
        TipoAliado = a.TipoAliado.NombreMostrable(),
        PaisAsignado = a.PaisAsignado?.Nombre ?? string.Empty,
        Activo = a.Activo
    };

    public static ClienteDetalleViewModel ADetalle(this Cliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Correo = c.Correo,
        Telefono = c.Telefono,
        CanalContacto = c.CanalContacto,
        TipoCliente = c.TipoCliente.NombreMostrable(),
        PaisOrigen = c.PaisOrigen?.Nombre ?? string.Empty,
        FechaRegistro = c.FechaRegistro,
        Tramites = c.Tramites.OrderByDescending(t => t.FechaSolicitud).Select(t => t.AItem()).ToList()
    };

    public static ClienteFormViewModel AForm(this Cliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Correo = c.Correo,
        Telefono = c.Telefono,
        CanalContacto = c.CanalContacto,
        TipoCliente = c.TipoCliente,
        PaisOrigenId = c.PaisOrigenId
    };

    public static Cliente AEntidad(this ClienteFormViewModel vm) => new()
    {
        Id = vm.Id,
        Nombre = vm.Nombre,
        Correo = vm.Correo,
        Telefono = vm.Telefono,
        CanalContacto = vm.CanalContacto,
        TipoCliente = vm.TipoCliente,
        PaisOrigenId = vm.PaisOrigenId ?? 0
    };

    public static AliadoFormViewModel AForm(this Aliado a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Correo = a.Correo,
        Telefono = a.Telefono,
        TipoAliado = a.TipoAliado,
        PaisAsignadoId = a.PaisAsignadoId,
        Activo = a.Activo
    };

    public static Aliado AEntidad(this AliadoFormViewModel vm) => new()
    {
        Id = vm.Id,
        Nombre = vm.Nombre,
        Correo = vm.Correo,
        Telefono = vm.Telefono,
        TipoAliado = vm.TipoAliado,
        PaisAsignadoId = vm.PaisAsignadoId ?? 0,
        Activo = vm.Activo
    };

    public static List<SelectListItem> APaisesSelect(this IEnumerable<Pais> paises, int? seleccionado = null) =>
        paises.Select(p => new SelectListItem(p.Nombre, p.Id.ToString(), p.Id == seleccionado)).ToList();

    public static List<SelectListItem> ATiposSelect(this IEnumerable<TipoDocumento> tipos, int? seleccionado = null) =>
        tipos.Select(t => new SelectListItem(t.Nombre, t.Id.ToString(), t.Id == seleccionado)).ToList();

    public static List<SelectListItem> AClientesSelect(this IEnumerable<Cliente> clientes, int? seleccionado = null) =>
        clientes.Select(c => new SelectListItem($"{c.Nombre} ({c.Correo})", c.Id.ToString(), c.Id == seleccionado)).ToList();

    public static List<SelectListItem> AAliadosSelect(this IEnumerable<Aliado> aliados) =>
        aliados.Select(a => new SelectListItem($"{a.Nombre} — {a.TipoAliado.NombreMostrable()}", a.Id.ToString())).ToList();
}
