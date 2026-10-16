using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.ViewModels;

namespace AliadaLegal.Tests;

public class CrearClienteViewModelTests
{
    private static List<ValidationResult> Validar(
        CrearClienteViewModel modelo)
    {
        var resultados = new List<ValidationResult>();

        var contexto = new ValidationContext(modelo);

        Validator.TryValidateObject(
            modelo,
            contexto,
            resultados,
            validateAllProperties: true);

        return resultados;
    }

    [Fact]
    public void ClienteSinNombre_DebeSerInvalido()
    {
        var cliente = CrearClienteValido();
        cliente.Nombre = "";

        var errores = Validar(cliente);

        Assert.NotEmpty(errores);
        Assert.Contains(errores, e =>
            e.MemberNames.Contains("Nombre"));
    }

    [Fact]
    public void CorreoIncorrecto_DebeSerInvalido()
    {
        var cliente = CrearClienteValido();
        cliente.Email = "correo-invalido";

        var errores = Validar(cliente);

        Assert.Contains(errores, e =>
            e.MemberNames.Contains("Email"));
    }

    [Fact]
    public void TipoClienteCero_DebeSerInvalido()
    {
        var cliente = CrearClienteValido();
        cliente.TipoCliente = (TipoCliente)0;

        var errores = Validar(cliente);

        Assert.Contains(errores, e =>
            e.MemberNames.Contains("TipoCliente"));
    }

    [Fact]
    public void ClienteValido_NoDebeTenerErrores()
    {
        var cliente = CrearClienteValido();

        var errores = Validar(cliente);

        Assert.Empty(errores);
    }

    private static CrearClienteViewModel CrearClienteValido()
    {
        return new CrearClienteViewModel
        {
            Nombre = "Cliente de Prueba",
            Email = "cliente@example.com",
            Telefono = "78901234",
            TipoCliente = TipoCliente.Individual
        };
    }
}