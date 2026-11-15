using System.Text.RegularExpressions;
using AliadaLegal.Tests.Falsos;
using AliadaLegal.Web.Services;

namespace AliadaLegal.Tests.Servicios;

public class CodigoSeguimientoGeneradorTests
{
    [Fact]
    public void Formato_EsAL_Anio_SeisCaracteresSinAmbiguos()
    {
        var generador = new CodigoSeguimientoGenerador(new RelojFijo(2027));

        for (var i = 0; i < 200; i++)
            Assert.Matches(new Regex("^AL-2027-[ABCDEFGHJKLMNPQRSTUVWXYZ23456789]{6}$"), generador.Generar());
    }

    [Fact]
    public void Codigos_NoSeRepitenEnMuestraPequena()
    {
        var generador = new CodigoSeguimientoGenerador(new RelojFijo());

        var codigos = Enumerable.Range(0, 500).Select(_ => generador.Generar()).ToList();

        Assert.Equal(codigos.Count, codigos.Distinct().Count());
    }
}
