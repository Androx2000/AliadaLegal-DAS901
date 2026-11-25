using System.Security.Claims;
using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Controllers.Api;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;

namespace AliadaLegal.Tests.Api;

/// <summary>
/// Pruebas del controlador API de trámites: códigos HTTP, DTOs y la restricción
/// de que un usuario Aliado solo vea y avance sus propios trámites.
/// </summary>
public class TramitesGestionApiControllerTests
{
    private readonly TramiteRepositorioFalso _tramites = new();
    private readonly ClienteRepositorioFalso _clientes = new();
    private readonly AliadoRepositorioFalso _aliados = new();
    private readonly CatalogoRepositorioFalso _catalogos = new();

    public TramitesGestionApiControllerTests()
    {
        _clientes.Datos.Add(new Cliente { Id = 1, Nombre = "Ana", Correo = "ana@correo.com" });
        _aliados.Datos.Add(new Aliado { Id = 10, Nombre = "Lic. Pérez", PaisAsignadoId = 1, Activo = true });
        _aliados.Datos.Add(new Aliado { Id = 11, Nombre = "Lic. Gómez", PaisAsignadoId = 2, Activo = true });
    }

    private static ClaimsPrincipal Asesor() => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "Marcos (Asesor)"), new Claim(ClaimTypes.Role, Roles.Asesor)
    }, "Prueba"));

    private static ClaimsPrincipal UsuarioAliado(int aliadoId) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.Name, "Carolina (Aliada)"), new Claim(ClaimTypes.Role, Roles.Aliado),
        new Claim(UsuarioActualExtensions.ClaimAliadoId, aliadoId.ToString())
    }, "Prueba"));

    private TramitesGestionApiController Crear(ClaimsPrincipal? usuario = null)
    {
        var servicio = new TramiteService(_tramites, _clientes, _aliados, _catalogos,
            new GeneradorFijo("AL-2026-AAAAAA", "AL-2026-BBBBBB"), new RelojFijo());
        return new(servicio, NullLogger<TramitesGestionApiController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = usuario ?? Asesor() }
            },
            ProblemDetailsFactory = new ProblemDetailsFactoryFalsa()
        };
    }

    private Tramite Sembrar(EstadoTramite estado, bool requiereApostilla = false, int? aliadoId = 10)
    {
        var t = new Tramite
        {
            Id = _tramites.Datos.Count + 1,
            CodigoSeguimiento = $"AL-2026-T{_tramites.Datos.Count + 1:00000}",
            ClienteId = 1,
            PaisDestinoId = 1,
            TipoDocumentoId = requiereApostilla ? 2 : 1,
            TipoDocumento = requiereApostilla ? _catalogos.Tipos[1] : _catalogos.Tipos[0],
            Estado = estado,
            AliadoId = aliadoId
        };
        _tramites.Datos.Add(t);
        return t;
    }

    private static int? Estado(IActionResult r) => (r as IStatusCodeActionResult)?.StatusCode;

    // ------------------------------------------------------------ Consultas
    [Fact]
    public async Task Listar_UsuarioAliado_SoloRecibeSusTramites()
    {
        Sembrar(EstadoTramite.EnGestion, aliadoId: 10);
        Sembrar(EstadoTramite.EnGestion, aliadoId: 11);

        // Aunque pida los del aliado 11, el filtro se fuerza a su propia ficha.
        var r = await Crear(UsuarioAliado(10)).Listar(null, null, null, aliadoId: 11);

        var lista = Assert.IsAssignableFrom<IEnumerable<TramiteDto>>(Assert.IsType<OkObjectResult>(r.Result).Value);
        Assert.All(lista, t => Assert.Equal(10, t.AliadoId));
    }

    [Fact]
    public async Task Obtener_DevuelveDetalleConProgresoYSiguienteEstado()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido, requiereApostilla: true);

        var r = await Crear().Obtener(t.Id);

        var dto = Assert.IsType<TramiteDetalleDto>(Assert.IsType<OkObjectResult>(r.Result).Value);
        Assert.Equal("DocumentoObtenido", dto.Estado);
        Assert.Equal("Apostillado", dto.SiguienteEstado);
        Assert.Equal(40, dto.Progreso); // 2 de 5 pasos del recorrido con apostilla
    }

    [Fact]
    public async Task Obtener_TramiteDeOtroAliado_404()
    {
        var t = Sembrar(EstadoTramite.EnGestion, aliadoId: 11);

        var r = await Crear(UsuarioAliado(10)).Obtener(t.Id);

        Assert.Equal(404, Estado(r.Result!));
    }

    // ------------------------------------------------------ Crear / editar / eliminar
    [Fact]
    public async Task Crear_Valido_201EnSolicitadoConCodigo()
    {
        var r = await Crear().Crear(new TramiteGuardarDto { ClienteId = 1, TipoDocumentoId = 1, PaisDestinoId = 1 });

        var creado = Assert.IsType<CreatedAtActionResult>(r.Result);
        var dto = Assert.IsType<TramiteDto>(creado.Value);
        Assert.Equal("Solicitado", dto.Estado);
        Assert.Equal("AL-2026-AAAAAA", dto.CodigoSeguimiento);
        Assert.Equal("Marcos (Asesor)", Assert.Single(_tramites.Datos[0].Historial).Responsable);
    }

    [Fact]
    public async Task Crear_ClienteInexistente_400()
    {
        var r = await Crear().Crear(new TramiteGuardarDto { ClienteId = 99, TipoDocumentoId = 1, PaisDestinoId = 1 });

        Assert.Equal(400, Estado(r.Result!));
        Assert.Empty(_tramites.Datos);
    }

    [Fact]
    public async Task Actualizar_TramiteQueYaAvanzo_400()
    {
        var t = Sembrar(EstadoTramite.EnGestion);

        var r = await Crear().Actualizar(t.Id, new TramiteGuardarDto { ClienteId = 1, TipoDocumentoId = 1, PaisDestinoId = 1 });

        Assert.Equal(400, Estado(r));
    }

    [Fact]
    public async Task Eliminar_Solicitado_204()
    {
        var t = Sembrar(EstadoTramite.Solicitado, aliadoId: null);

        Assert.IsType<NoContentResult>(await Crear().Eliminar(t.Id));
        Assert.Empty(_tramites.Datos);
    }

    [Fact]
    public async Task Eliminar_ConGestion_409YSeConserva()
    {
        var t = Sembrar(EstadoTramite.EnGestion);

        Assert.Equal(409, Estado(await Crear().Eliminar(t.Id)));
        Assert.Single(_tramites.Datos);
    }

    [Fact]
    public async Task Eliminar_Inexistente_404()
    {
        Assert.Equal(404, Estado(await Crear().Eliminar(99)));
    }

    // ------------------------------------------------------------ Flujo
    [Fact]
    public async Task AsignarAliado_Solicitado_204YPasaAEnGestion()
    {
        var t = Sembrar(EstadoTramite.Solicitado, aliadoId: null);

        var r = await Crear().AsignarAliado(t.Id, new AsignarAliadoDto { AliadoId = 10 });

        Assert.IsType<NoContentResult>(r);
        Assert.Equal(EstadoTramite.EnGestion, t.Estado);
        Assert.Equal(10, t.AliadoId);
    }

    [Fact]
    public async Task AsignarAliado_DeOtroPais_400()
    {
        var t = Sembrar(EstadoTramite.Solicitado, aliadoId: null);

        var r = await Crear().AsignarAliado(t.Id, new AsignarAliadoDto { AliadoId = 11 });

        Assert.Equal(400, Estado(r));
        Assert.Null(t.AliadoId);
    }

    [Fact]
    public async Task CambiarEstado_AliadoSobreSuTramite_204()
    {
        var t = Sembrar(EstadoTramite.EnGestion, aliadoId: 10);

        var r = await Crear(UsuarioAliado(10)).CambiarEstado(t.Id,
            new CambiarEstadoDto { NuevoEstado = EstadoTramite.DocumentoObtenido, Comentario = "Listo" });

        Assert.IsType<NoContentResult>(r);
        Assert.Equal(EstadoTramite.DocumentoObtenido, t.Estado);
        Assert.Equal("Carolina (Aliada)", t.Historial.Last().Responsable);
    }

    [Fact]
    public async Task CambiarEstado_AliadoSobreTramiteAjeno_404()
    {
        var t = Sembrar(EstadoTramite.EnGestion, aliadoId: 11);

        var r = await Crear(UsuarioAliado(10)).CambiarEstado(t.Id,
            new CambiarEstadoDto { NuevoEstado = EstadoTramite.DocumentoObtenido });

        Assert.Equal(404, Estado(r));
        Assert.Equal(EstadoTramite.EnGestion, t.Estado);
    }

    [Fact]
    public async Task CambiarEstado_SaltandoPasos_400()
    {
        var t = Sembrar(EstadoTramite.EnGestion);

        var r = await Crear().CambiarEstado(t.Id, new CambiarEstadoDto { NuevoEstado = EstadoTramite.Entregado });

        Assert.Equal(400, Estado(r));
        Assert.Equal(EstadoTramite.EnGestion, t.Estado);
    }

    [Fact]
    public async Task RegistrarEnvio_DocumentoObtenido_204YPasaAEnTransito()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido);

        var r = await Crear().RegistrarEnvio(t.Id, new RegistrarEnvioDto
        {
            NumeroGuia = "GU-123", EmpresaEnvio = "DHL", FechaDespacho = new DateTime(2026, 11, 20)
        });

        Assert.IsType<NoContentResult>(r);
        Assert.Equal(EstadoTramite.EnTransito, t.Estado);
        Assert.Equal("GU-123", t.Envio!.NumeroGuia);
    }

    [Fact]
    public async Task RegistrarEnvio_SinApostillaRequerida_400()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido, requiereApostilla: true);

        var r = await Crear().RegistrarEnvio(t.Id, new RegistrarEnvioDto
        {
            NumeroGuia = "GU-123", EmpresaEnvio = "DHL", FechaDespacho = new DateTime(2026, 11, 20)
        });

        Assert.Equal(400, Estado(r));
        Assert.Null(t.Envio);
    }
}
