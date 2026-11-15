using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Services;

namespace AliadaLegal.Tests.Servicios;

public class SeguimientoServiceTests
{
    private readonly TramiteRepositorioFalso _repo = new();

    private Tramite Sembrar(EstadoTramite estado, bool apostilla)
    {
        var t = new Tramite
        {
            Id = 1,
            CodigoSeguimiento = "AL-2026-K7M2QX",
            Estado = estado,
            TipoDocumento = new TipoDocumento { Nombre = "Antecedentes", RequiereApostilla = apostilla },
            PaisDestino = new Pais { Nombre = "Guatemala", CodigoIso = "GT" },
            FechaSolicitud = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        t.Historial.Add(new HistorialEstado
        {
            EstadoNuevo = EstadoTramite.Solicitado,
            Fecha = t.FechaSolicitud,
            Responsable = "asesor"
        });
        _repo.Datos.Add(t);
        return t;
    }

    [Fact]
    public async Task CodigoInexistente_DevuelveNull()
    {
        var r = await new SeguimientoService(_repo).ConsultarAsync("AL-2026-NOEXIS");
        Assert.Null(r);
    }

    [Fact]
    public async Task CodigoVacio_DevuelveNull()
    {
        Assert.Null(await new SeguimientoService(_repo).ConsultarAsync("   "));
    }

    [Fact]
    public async Task IgnoraMayusculasYEspacios()
    {
        Sembrar(EstadoTramite.EnGestion, false);

        var r = await new SeguimientoService(_repo).ConsultarAsync("  al-2026-k7m2qx ");

        Assert.NotNull(r);
        Assert.Equal("AL-2026-K7M2QX", r!.CodigoSeguimiento);
    }

    [Fact]
    public async Task Entregado_TieneProgresoCompleto()
    {
        Sembrar(EstadoTramite.Entregado, false);

        var r = await new SeguimientoService(_repo).ConsultarAsync("AL-2026-K7M2QX");

        Assert.Equal(100, r!.Progreso);
        Assert.All(r.LineaDeTiempo, h => Assert.True(h.Completado));
    }

    [Fact]
    public async Task LineaDeTiempo_IncluyeApostillaSoloSiElDocumentoLaRequiere()
    {
        Sembrar(EstadoTramite.Solicitado, apostilla: false);
        var sin = await new SeguimientoService(_repo).ConsultarAsync("AL-2026-K7M2QX");

        _repo.Datos.Clear();
        Sembrar(EstadoTramite.Solicitado, apostilla: true);
        var con = await new SeguimientoService(_repo).ConsultarAsync("AL-2026-K7M2QX");

        Assert.Equal(5, sin!.LineaDeTiempo.Count);
        Assert.Equal(6, con!.LineaDeTiempo.Count);
    }

    [Fact]
    public async Task EstadoActual_QuedaMarcadoEnLaLineaDeTiempo()
    {
        Sembrar(EstadoTramite.EnGestion, false);

        var r = await new SeguimientoService(_repo).ConsultarAsync("AL-2026-K7M2QX");

        var actual = Assert.Single(r!.LineaDeTiempo, h => h.EsActual);
        Assert.Equal("engestion", actual.EstadoClave);
    }
}
