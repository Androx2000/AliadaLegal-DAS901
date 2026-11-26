using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Controllers.Api;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace AliadaLegal.Tests.Api;

/// <summary>Pruebas del controlador API de aliados, incluida la baja lógica.</summary>
public class AliadosApiControllerTests
{
    private readonly AliadoRepositorioFalso _aliados = new();
    private readonly CatalogoRepositorioFalso _catalogos = new();

    private AliadosApiController Crear() =>
        new(new AliadoService(_aliados, _catalogos), NullLogger<AliadosApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            ProblemDetailsFactory = new ProblemDetailsFactoryFalsa()
        };

    private Aliado Sembrar(int paisId = 1, bool activo = true, string correo = "perez@bufete.com")
    {
        var a = new Aliado
        {
            Id = _aliados.Datos.Count + 1, Nombre = "Lic. Pérez", Correo = correo, Telefono = "7777-0000",
            TipoAliado = TipoAliado.Abogado, PaisAsignadoId = paisId, Activo = activo,
            PaisAsignado = _catalogos.Paises.First(p => p.Id == paisId)
        };
        _aliados.Datos.Add(a);
        return a;
    }

    private static AliadoGuardarDto Dto(string correo = "nuevo@notaria.com", int paisId = 1, bool activo = true) => new()
    {
        Nombre = "Notario Rojas", Correo = correo, Telefono = "2222-1111",
        TipoAliado = TipoAliado.Notario, PaisAsignadoId = paisId, Activo = activo
    };

    private static int? Estado(IActionResult r) => (r as IStatusCodeActionResult)?.StatusCode;

    private static List<AliadoDto> Lista(ActionResult<IEnumerable<AliadoDto>> r) =>
        Assert.IsAssignableFrom<IEnumerable<AliadoDto>>(Assert.IsType<OkObjectResult>(r.Result).Value).ToList();

    [Fact]
    public async Task Listar_SoloActivos_ExcluyeBajas()
    {
        Sembrar();
        Sembrar(activo: false, correo: "baja@bufete.com");

        var todos = Lista(await Crear().Listar());
        var activos = Lista(await Crear().Listar(soloActivos: true));

        Assert.Equal(2, todos.Count);
        Assert.Single(activos);
    }

    [Fact]
    public async Task Listar_PorPaisYSoloActivos_CombinaFiltros()
    {
        Sembrar(paisId: 1);
        Sembrar(paisId: 1, activo: false, correo: "baja@bufete.com");
        Sembrar(paisId: 2, correo: "gt@bufete.com");

        var r = Lista(await Crear().Listar(soloActivos: true, paisId: 1));

        var dto = Assert.Single(r);
        Assert.Equal("El Salvador", dto.PaisAsignado);
        Assert.True(dto.Activo);
    }

    [Fact]
    public async Task Obtener_Inexistente_404()
    {
        var r = await Crear().Obtener(99);

        Assert.Equal(404, Estado(r.Result!));
    }

    [Fact]
    public async Task Crear_Valido_201()
    {
        var r = await Crear().Crear(Dto());

        var creado = Assert.IsType<CreatedAtActionResult>(r.Result);
        Assert.Equal("Notario", Assert.IsType<AliadoDto>(creado.Value).TipoAliado);
        Assert.Single(_aliados.Datos);
    }

    [Fact]
    public async Task Crear_PaisInexistente_400()
    {
        var r = await Crear().Crear(Dto(paisId: 999));

        Assert.Equal(400, Estado(r.Result!));
        Assert.Empty(_aliados.Datos);
    }

    [Fact]
    public async Task Actualizar_CambiarPaisConTramitesAbiertos_400()
    {
        var a = Sembrar();
        _aliados.TramitesAbiertos = 2;

        var r = await Crear().Actualizar(a.Id, Dto(a.Correo, paisId: 2));

        Assert.Equal(400, Estado(r));
        Assert.Equal(1, a.PaisAsignadoId);
    }

    [Fact]
    public async Task Actualizar_Inexistente_404()
    {
        Assert.Equal(404, Estado(await Crear().Actualizar(99, Dto())));
    }

    [Fact]
    public async Task DarDeBaja_SinTramitesAbiertos_204YQuedaInactivoSinBorrar()
    {
        var a = Sembrar();

        var r = await Crear().DarDeBaja(a.Id);

        Assert.IsType<NoContentResult>(r);
        Assert.False(a.Activo);
        Assert.Single(_aliados.Datos); // baja lógica: el registro se conserva
    }

    [Fact]
    public async Task DarDeBaja_ConTramitesAbiertos_409YSigueActivo()
    {
        var a = Sembrar();
        _aliados.TramitesAbiertos = 1;

        var r = await Crear().DarDeBaja(a.Id);

        Assert.Equal(409, Estado(r));
        Assert.True(a.Activo);
    }

    [Fact]
    public async Task DarDeBaja_YaInactivo_204Idempotente()
    {
        var a = Sembrar(activo: false);
        _aliados.TramitesAbiertos = 1; // aunque tuviera abiertos, no hay nada que cambiar

        Assert.IsType<NoContentResult>(await Crear().DarDeBaja(a.Id));
    }

    [Fact]
    public async Task DarDeBaja_Inexistente_404()
    {
        Assert.Equal(404, Estado(await Crear().DarDeBaja(99)));
    }
}
