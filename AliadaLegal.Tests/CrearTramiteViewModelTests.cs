using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.ViewModels;

namespace AliadaLegal.Tests;

public class CrearTramiteViewModelTests
{
    private static List<ValidationResult> Validar(
        CrearTramiteViewModel modelo)
    {
        var errores = new List<ValidationResult>();
        var contexto = new ValidationContext(modelo);

        Validator.TryValidateObject(
            modelo,
            contexto,
            errores,
            validateAllProperties: true);

        return errores;
    }

    private static CrearTramiteViewModel CrearTramiteValido()
    {
        return new CrearTramiteViewModel
        {
            ClienteId = 1,
            PaisId = 1,
            TipoDocumentoId = 1,
            Observaciones = "Solicitud de partida de nacimiento"
        };
    }

    [Fact]
    public void ClienteIdCero_DebeSerInvalido()
    {
        var tramite = CrearTramiteValido();
        tramite.ClienteId = 0;

        var errores = Validar(tramite);

        Assert.Contains(errores, e =>
            e.MemberNames.Contains("ClienteId"));
    }

    [Fact]
    public void PaisIdNegativo_DebeSerInvalido()
    {
        var tramite = CrearTramiteValido();
        tramite.PaisId = -1;

        var errores = Validar(tramite);

        Assert.Contains(errores, e =>
            e.MemberNames.Contains("PaisId"));
    }

    [Fact]
    public void TipoDocumentoIdCero_DebeSerInvalido()
    {
        var tramite = CrearTramiteValido();
        tramite.TipoDocumentoId = 0;

        var errores = Validar(tramite);

        Assert.Contains(errores, e =>
            e.MemberNames.Contains("TipoDocumentoId"));
    }

    [Fact]
    public void ObservacionesMuyLargas_DebeSerInvalido()
    {
        var tramite = CrearTramiteValido();
        tramite.Observaciones = new string('A', 1001);

        var errores = Validar(tramite);

        Assert.Contains(errores, e =>
            e.MemberNames.Contains("Observaciones"));
    }

    [Fact]
    public void TramiteValido_NoDebeTenerErrores()
    {
        var tramite = CrearTramiteValido();

        var errores = Validar(tramite);

        Assert.Empty(errores);
    }
}