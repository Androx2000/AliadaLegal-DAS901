using AliadaLegal.Web.Models;

namespace AliadaLegal.Tests.Modelos;

/// <summary>
/// Verifica que Tramite.PuedeCambiarA use la misma regla que EstadoTramiteFlujo
/// (antes tenía una regla duplicada que ignoraba la apostilla).
/// </summary>
public class TramiteTests
{
    private static Tramite Nuevo(EstadoTramite estado, bool requiereApostilla) => new()
    {
        Estado = estado,
        TipoDocumento = new TipoDocumento { Id = 1, Nombre = "Doc", RequiereApostilla = requiereApostilla }
    };

    [Theory]
    [InlineData(EstadoTramite.Solicitado, EstadoTramite.EnGestion, false, true)]
    [InlineData(EstadoTramite.DocumentoObtenido, EstadoTramite.Apostillado, true, true)]
    [InlineData(EstadoTramite.DocumentoObtenido, EstadoTramite.EnTransito, false, true)]
    [InlineData(EstadoTramite.DocumentoObtenido, EstadoTramite.Apostillado, false, false)] // la regla vieja lo permitía
    [InlineData(EstadoTramite.DocumentoObtenido, EstadoTramite.EnTransito, true, false)]  // no se salta la apostilla
    [InlineData(EstadoTramite.Solicitado, EstadoTramite.Entregado, false, false)]
    [InlineData(EstadoTramite.Entregado, EstadoTramite.Solicitado, false, false)]
    public void PuedeCambiarA_CoincideConEstadoTramiteFlujo(
        EstadoTramite actual, EstadoTramite nuevo, bool requiereApostilla, bool esperado)
    {
        var tramite = Nuevo(actual, requiereApostilla);

        Assert.Equal(esperado, tramite.PuedeCambiarA(nuevo));
        Assert.Equal(EstadoTramiteFlujo.EsTransicionValida(actual, nuevo, requiereApostilla),
            tramite.PuedeCambiarA(nuevo));
    }

    [Fact]
    public void ActualizarEstado_TransicionInvalida_LanzaYNoCambiaNada()
    {
        var tramite = Nuevo(EstadoTramite.DocumentoObtenido, requiereApostilla: false);

        Assert.Throws<InvalidOperationException>(() =>
            tramite.ActualizarEstado(EstadoTramite.Apostillado, "asesor", null));
        Assert.Equal(EstadoTramite.DocumentoObtenido, tramite.Estado);
        Assert.Empty(tramite.Historial);
    }
}
