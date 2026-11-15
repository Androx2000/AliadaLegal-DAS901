using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;

namespace AliadaLegal.Tests.Servicios;

public class TramiteServiceTests
{
    private readonly TramiteRepositorioFalso _tramites = new();
    private readonly ClienteRepositorioFalso _clientes = new();
    private readonly AliadoRepositorioFalso _aliados = new();
    private readonly CatalogoRepositorioFalso _catalogos = new();
    private readonly RelojFijo _reloj = new();

    public TramiteServiceTests()
    {
        _clientes.Datos.Add(new Cliente { Id = 1, Nombre = "Ana", Correo = "ana@correo.com" });
        _aliados.Datos.Add(new Aliado { Id = 10, Nombre = "Lic. Pérez", PaisAsignadoId = 1, Activo = true });
        _aliados.Datos.Add(new Aliado { Id = 11, Nombre = "Lic. Gómez", PaisAsignadoId = 2, Activo = true });
    }

    private TramiteService Crear(params string[] codigos) =>
        new(_tramites, _clientes, _aliados, _catalogos,
            new GeneradorFijo(codigos.Length > 0 ? codigos : new[] { "AL-2026-AAAAAA" }), _reloj);

    private Tramite Sembrar(EstadoTramite estado, bool requiereApostilla, int? aliadoId = 10)
    {
        var tramite = new Tramite
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
        _tramites.Datos.Add(tramite);
        return tramite;
    }

    // ---------------------------------------------------------- Crear
    [Fact]
    public async Task Crear_ClienteInexistente_Falla()
    {
        var r = await Crear().CrearAsync(99, 1, 1, null, "asesor");

        Assert.False(r.Exito);
        Assert.Contains("cliente", r.Error);
        Assert.Empty(_tramites.Datos);
    }

    [Fact]
    public async Task Crear_Valido_RegistraSolicitadoConHistorialYCodigo()
    {
        var r = await Crear("AL-2026-K7M2QX").CrearAsync(1, 1, 1, "  urgente  ", "asesor");

        Assert.True(r.Exito);
        var t = Assert.Single(_tramites.Datos);
        Assert.Equal("AL-2026-K7M2QX", t.CodigoSeguimiento);
        Assert.Equal(EstadoTramite.Solicitado, t.Estado);
        Assert.Equal("urgente", t.Observaciones);
        Assert.Equal(_reloj.GetUtcNow().UtcDateTime, t.FechaSolicitud);
        var h = Assert.Single(t.Historial);
        Assert.Null(h.EstadoAnterior);
        Assert.Equal("asesor", h.Responsable);
        Assert.Equal(1, _tramites.Guardados);
    }

    [Fact]
    public async Task Crear_CodigoRepetido_PideOtroAlGenerador()
    {
        Sembrar(EstadoTramite.Solicitado, false).CodigoSeguimiento = "AL-2026-REPETI";

        var r = await Crear("AL-2026-REPETI", "AL-2026-NUEVOO").CrearAsync(1, 1, 1, null, "asesor");

        Assert.True(r.Exito);
        Assert.Equal("AL-2026-NUEVOO", r.Valor!.CodigoSeguimiento);
    }

    // -------------------------------------------------------- Asignar
    [Fact]
    public async Task Asignar_AliadoDeOtroPais_Falla()
    {
        var t = Sembrar(EstadoTramite.Solicitado, false, aliadoId: null);

        var r = await Crear().AsignarAliadoAsync(t.Id, 11, "asesor");

        Assert.False(r.Exito);
        Assert.Null(t.AliadoId);
    }

    [Fact]
    public async Task Asignar_PrimeraVez_PasaAEnGestion()
    {
        var t = Sembrar(EstadoTramite.Solicitado, false, aliadoId: null);

        var r = await Crear().AsignarAliadoAsync(t.Id, 10, "asesor");

        Assert.True(r.Exito);
        Assert.Equal(10, t.AliadoId);
        Assert.Equal(EstadoTramite.EnGestion, t.Estado);
        Assert.Single(t.Historial);
    }

    [Fact]
    public async Task Asignar_TramiteEntregado_Falla()
    {
        var t = Sembrar(EstadoTramite.Entregado, false);

        var r = await Crear().AsignarAliadoAsync(t.Id, 10, "asesor");

        Assert.False(r.Exito);
    }

    // ---------------------------------------------------- Cambiar estado
    [Fact]
    public async Task CambiarEstado_SaltandoPasos_Falla()
    {
        var t = Sembrar(EstadoTramite.EnGestion, false);

        var r = await Crear().CambiarEstadoAsync(t.Id, EstadoTramite.Entregado, null, "asesor", null);

        Assert.False(r.Exito);
        Assert.Equal(EstadoTramite.EnGestion, t.Estado);
    }

    [Fact]
    public async Task CambiarEstado_SiguientePaso_Avanza()
    {
        var t = Sembrar(EstadoTramite.EnGestion, false);

        var r = await Crear().CambiarEstadoAsync(t.Id, EstadoTramite.DocumentoObtenido, "listo", "asesor", null);

        Assert.True(r.Exito);
        Assert.Equal(EstadoTramite.DocumentoObtenido, t.Estado);
        Assert.Equal("listo", t.Historial.Single().Comentario);
    }

    [Fact]
    public async Task CambiarEstado_ApostilladoEnDocumentoQueNoLoRequiere_Falla()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido, requiereApostilla: false);

        var r = await Crear().CambiarEstadoAsync(t.Id, EstadoTramite.Apostillado, null, "asesor", null);

        Assert.False(r.Exito);
    }

    [Fact]
    public async Task CambiarEstado_AliadoSobreTramiteAjeno_Falla()
    {
        var t = Sembrar(EstadoTramite.EnGestion, false, aliadoId: 10);

        var r = await Crear().CambiarEstadoAsync(t.Id, EstadoTramite.DocumentoObtenido, null, "aliado", aliadoIdRestriccion: 11);

        Assert.False(r.Exito);
        Assert.Equal(EstadoTramite.EnGestion, t.Estado);
    }

    [Fact]
    public async Task CambiarEstado_ATransitoSinRegistrarEnvio_Falla()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido, false);

        var r = await Crear().CambiarEstadoAsync(t.Id, EstadoTramite.EnTransito, null, "asesor", null);

        Assert.False(r.Exito);
        Assert.Contains("guía", r.Error);
    }

    // ------------------------------------------------------------ Envío
    [Fact]
    public async Task RegistrarEnvio_DocumentoConApostillaSinApostillar_Falla()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido, requiereApostilla: true);

        var r = await Crear().RegistrarEnvioAsync(t.Id, "GUIA-1", "DHL", _reloj.GetUtcNow().UtcDateTime, "asesor");

        Assert.False(r.Exito);
        Assert.Null(t.Envio);
    }

    [Fact]
    public async Task RegistrarEnvio_Valido_GuardaGuiaYPasaAEnTransito()
    {
        var t = Sembrar(EstadoTramite.DocumentoObtenido, requiereApostilla: false);

        var r = await Crear().RegistrarEnvioAsync(t.Id, " GUIA-1 ", " DHL ", _reloj.GetUtcNow().UtcDateTime, "asesor");

        Assert.True(r.Exito);
        Assert.Equal(EstadoTramite.EnTransito, t.Estado);
        Assert.Equal("GUIA-1", t.Envio!.NumeroGuia);
        Assert.Equal("DHL", t.Envio.EmpresaEnvio);
    }

    // --------------------------------------------------------- Consultas
    [Fact]
    public async Task Resumen_CuentaActivosYEntregados()
    {
        Sembrar(EstadoTramite.Solicitado, false);
        Sembrar(EstadoTramite.EnGestion, false);
        Sembrar(EstadoTramite.Entregado, false);

        var resumen = await Crear().ObtenerResumenAsync(null);

        Assert.Equal(3, resumen.Total);
        Assert.Equal(1, resumen.Entregados);
        Assert.Equal(2, resumen.Activos);
    }

    [Fact]
    public async Task Obtener_AliadoSoloVeSusTramites()
    {
        var t = Sembrar(EstadoTramite.EnGestion, false, aliadoId: 10);
        var servicio = Crear();

        Assert.NotNull(await servicio.ObtenerAsync(t.Id, 10));
        Assert.Null(await servicio.ObtenerAsync(t.Id, 11));
    }
}
