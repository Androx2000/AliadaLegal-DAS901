using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;

namespace AliadaLegal.Tests.Servicios;

public class ClienteServiceTests
{
    private readonly ClienteRepositorioFalso _clientes = new();
    private readonly CatalogoRepositorioFalso _catalogos = new();

    private ClienteService Crear() => new(_clientes, _catalogos);

    private static Cliente Nuevo(string correo = "  ANA@Correo.com ", int pais = 1) => new()
    {
        Nombre = "  Ana  ",
        Correo = correo,
        Telefono = "7000-0000",
        PaisOrigenId = pais
    };

    [Fact]
    public async Task Crear_NormalizaDatosYRegistraFecha()
    {
        var r = await Crear().CrearAsync(Nuevo());

        Assert.True(r.Exito);
        Assert.Equal("ana@correo.com", r.Valor!.Correo);
        Assert.Equal("Ana", r.Valor.Nombre);
        Assert.True(Math.Abs((r.Valor.FechaRegistro - DateTime.UtcNow).TotalMinutes) < 1);
    }

    [Fact]
    public async Task Crear_CorreoDuplicado_Falla()
    {
        await Crear().CrearAsync(Nuevo());

        var r = await Crear().CrearAsync(Nuevo());

        Assert.False(r.Exito);
        Assert.Single(_clientes.Datos);
    }

    [Fact]
    public async Task Crear_PaisInexistente_Falla()
    {
        var r = await Crear().CrearAsync(Nuevo(pais: 99));

        Assert.False(r.Exito);
    }

    [Fact]
    public async Task Actualizar_ClienteInexistente_Falla()
    {
        var r = await Crear().ActualizarAsync(new Cliente { Id = 5, Correo = "x@x.com", Nombre = "X", Telefono = "1" });

        Assert.False(r.Exito);
    }
}

public class AliadoServiceTests
{
    private readonly AliadoRepositorioFalso _aliados = new();
    private readonly CatalogoRepositorioFalso _catalogos = new();

    private AliadoService Crear() => new(_aliados, _catalogos);

    private Aliado Sembrar()
    {
        var a = new Aliado
        {
            Id = 1, Nombre = "Lic. Pérez", Correo = "perez@aliado.com", Telefono = "1",
            PaisAsignadoId = 1, Activo = true, TipoAliado = TipoAliado.Abogado
        };
        _aliados.Datos.Add(a);
        return a;
    }

    [Fact]
    public async Task Desactivar_ConTramitesAbiertos_Falla()
    {
        var a = Sembrar();
        _aliados.TramitesAbiertos = 2;

        var r = await Crear().ActualizarAsync(new Aliado
        {
            Id = a.Id, Nombre = a.Nombre, Correo = a.Correo, Telefono = a.Telefono,
            PaisAsignadoId = 1, Activo = false, TipoAliado = a.TipoAliado
        });

        Assert.False(r.Exito);
        Assert.True(a.Activo);
    }

    [Fact]
    public async Task Desactivar_SinTramitesAbiertos_Funciona()
    {
        var a = Sembrar();

        var r = await Crear().ActualizarAsync(new Aliado
        {
            Id = a.Id, Nombre = a.Nombre, Correo = a.Correo, Telefono = a.Telefono,
            PaisAsignadoId = 1, Activo = false, TipoAliado = a.TipoAliado
        });

        Assert.True(r.Exito);
        Assert.False(a.Activo);
    }

    [Fact]
    public async Task Crear_CorreoDuplicado_Falla()
    {
        Sembrar();

        var r = await Crear().CrearAsync(new Aliado
        {
            Nombre = "Otro", Correo = " PEREZ@aliado.com ", Telefono = "1", PaisAsignadoId = 1
        });

        Assert.False(r.Exito);
    }
}
