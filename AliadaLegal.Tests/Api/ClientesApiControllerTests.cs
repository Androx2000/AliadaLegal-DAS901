using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Controllers.Api;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging.Abstractions;

namespace AliadaLegal.Tests.Api;

/// <summary>
/// Pruebas del controlador API de clientes: verifican que cada resultado del
/// servicio se traduzca al código HTTP correcto y que se devuelvan DTOs.
/// </summary>
public class ClientesApiControllerTests
{
    private readonly ClienteRepositorioFalso _clientes = new();
    private readonly CatalogoRepositorioFalso _catalogos = new();

    private ClientesApiController Crear() =>
        new(new ClienteService(_clientes, _catalogos), NullLogger<ClientesApiController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
            ProblemDetailsFactory = new ProblemDetailsFactoryFalsa()
        };

    private Cliente Sembrar(string correo = "ana@correo.com")
    {
        var c = new Cliente
        {
            Id = _clientes.Datos.Count + 1, Nombre = "Ana", Correo = correo, Telefono = "7777-0000",
            TipoCliente = TipoCliente.Individual, PaisOrigenId = 1, PaisOrigen = _catalogos.Paises[0]
        };
        _clientes.Datos.Add(c);
        return c;
    }

    private static ClienteGuardarDto Dto(string correo = "nuevo@correo.com", int paisId = 1) => new()
    {
        Nombre = "Bufete López", Correo = correo, Telefono = "2222-1111",
        TipoCliente = TipoCliente.Bufete, PaisOrigenId = paisId, CanalContacto = "Web"
    };

    private static int? Estado(IActionResult r) => (r as IStatusCodeActionResult)?.StatusCode;

    [Fact]
    public async Task Listar_DevuelveDtos()
    {
        Sembrar();

        var r = await Crear().Listar(null);

        var ok = Assert.IsType<OkObjectResult>(r.Result);
        var dto = Assert.Single(Assert.IsAssignableFrom<IEnumerable<ClienteDto>>(ok.Value));
        Assert.Equal("Individual", dto.TipoCliente);
        Assert.Equal(_catalogos.Paises[0].Nombre, dto.PaisOrigen);
    }

    [Fact]
    public async Task Obtener_Existente_200()
    {
        var c = Sembrar();

        var r = await Crear().Obtener(c.Id);

        var dto = Assert.IsType<ClienteDto>(Assert.IsType<OkObjectResult>(r.Result).Value);
        Assert.Equal(c.Correo, dto.Correo);
    }

    [Fact]
    public async Task Obtener_Inexistente_404()
    {
        var r = await Crear().Obtener(99);

        Assert.Equal(404, Estado(r.Result!));
    }

    [Fact]
    public async Task Crear_Valido_201ConLocation()
    {
        var r = await Crear().Crear(Dto());

        var creado = Assert.IsType<CreatedAtActionResult>(r.Result);
        Assert.Equal(nameof(ClientesApiController.Obtener), creado.ActionName);
        var dto = Assert.IsType<ClienteDto>(creado.Value);
        Assert.Equal(dto.Id, creado.RouteValues!["id"]);
        Assert.Single(_clientes.Datos);
    }

    [Fact]
    public async Task Crear_CorreoRepetido_400YNoGuarda()
    {
        Sembrar("repetido@correo.com");

        var r = await Crear().Crear(Dto("REPETIDO@correo.com"));

        Assert.Equal(400, Estado(r.Result!));
        Assert.Single(_clientes.Datos);
    }

    [Fact]
    public async Task Actualizar_Existente_204YCambiaDatos()
    {
        var c = Sembrar();

        var r = await Crear().Actualizar(c.Id, Dto("ana.nueva@correo.com"));

        Assert.IsType<NoContentResult>(r);
        Assert.Equal("ana.nueva@correo.com", c.Correo);
    }

    [Fact]
    public async Task Actualizar_Inexistente_404()
    {
        var r = await Crear().Actualizar(99, Dto());

        Assert.Equal(404, Estado(r));
    }

    [Fact]
    public async Task Actualizar_PaisInexistente_400()
    {
        var c = Sembrar();

        var r = await Crear().Actualizar(c.Id, Dto(paisId: 999));

        Assert.Equal(400, Estado(r));
    }

    [Fact]
    public async Task Eliminar_SinTramites_204()
    {
        var c = Sembrar();

        var r = await Crear().Eliminar(c.Id);

        Assert.IsType<NoContentResult>(r);
        Assert.Empty(_clientes.Datos);
    }

    [Fact]
    public async Task Eliminar_ConTramites_409YConserva()
    {
        var c = Sembrar();
        _clientes.ConTramites = true;

        var r = await Crear().Eliminar(c.Id);

        Assert.Equal(409, Estado(r));
        Assert.Single(_clientes.Datos);
    }

    [Fact]
    public async Task Eliminar_Inexistente_404()
    {
        var r = await Crear().Eliminar(99);

        Assert.Equal(404, Estado(r));
    }
}

/// <summary>
/// En una prueba unitaria no hay contenedor de servicios, así que Problem() no encuentra
/// la fábrica de ProblemDetails. Esta versión mínima arma el objeto con los mismos datos.
/// </summary>
public class ProblemDetailsFactoryFalsa : ProblemDetailsFactory
{
    public override ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null,
        string? title = null, string? type = null, string? detail = null, string? instance = null) =>
        new() { Status = statusCode, Title = title, Type = type, Detail = detail, Instance = instance };

    public override ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext,
        ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null,
        string? type = null, string? detail = null, string? instance = null) =>
        new(modelStateDictionary) { Status = statusCode, Title = title, Type = type, Detail = detail, Instance = instance };
}
