using System.Text.Json;
using AliadaLegal.Web.Infraestructura;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace AliadaLegal.Tests.Api;

/// <summary>Pruebas del manejador global de excepciones de la API.</summary>
public class ApiExceptionHandlerTests
{
    private static ApiExceptionHandler Crear(string entorno)
    {
        var servicios = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        return new ApiExceptionHandler(servicios.GetRequiredService<Microsoft.AspNetCore.Http.IProblemDetailsService>(),
            new EntornoFalso(entorno), NullLogger<ApiExceptionHandler>.Instance);
    }

    private static DefaultHttpContext Peticion(string ruta)
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Method = "POST";
        contexto.Request.Path = ruta;
        contexto.Response.Body = new MemoryStream();
        contexto.RequestServices = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        return contexto;
    }

    private static JsonElement LeerCuerpo(HttpContext contexto)
    {
        contexto.Response.Body.Position = 0;
        return JsonDocument.Parse(contexto.Response.Body).RootElement;
    }

    [Fact]
    public async Task RutaMvc_NoLaAtiende()
    {
        var contexto = Peticion("/Clientes/Create");

        var atendida = await Crear(Environments.Production).TryHandleAsync(contexto, new Exception("x"), default);

        Assert.False(atendida);
    }

    [Fact]
    public async Task ErrorInesperado_EnProduccion_500SinRevelarElMensaje()
    {
        var contexto = Peticion("/api/clientes");

        var atendida = await Crear(Environments.Production)
            .TryHandleAsync(contexto, new InvalidOperationException("cadena de conexión secreta"), default);

        Assert.True(atendida);
        Assert.Equal(500, contexto.Response.StatusCode);
        var cuerpo = LeerCuerpo(contexto);
        Assert.Equal(500, cuerpo.GetProperty("status").GetInt32());
        Assert.DoesNotContain("secreta", cuerpo.GetRawText());
    }

    [Fact]
    public async Task ErrorInesperado_EnDesarrollo_MuestraElMensaje()
    {
        var contexto = Peticion("/api/clientes");

        await Crear(Environments.Development).TryHandleAsync(contexto, new Exception("detalle para depurar"), default);

        Assert.Equal("detalle para depurar", LeerCuerpo(contexto).GetProperty("detail").GetString());
    }

    [Fact]
    public void Concurrencia_Se_Clasifica_Como409()
    {
        var (codigo, _, _) = ApiExceptionHandler.Clasificar(new DbUpdateConcurrencyException("otro usuario"));

        Assert.Equal(409, codigo);
    }

    [Fact]
    public void DbUpdateException_SinIndiceUnico_EsErrorDelServidor()
    {
        // Solo los errores 2601/2627 de SQL Server son "duplicado"; cualquier otro fallo al guardar es 500.
        var (codigo, _, _) = ApiExceptionHandler.Clasificar(new DbUpdateException("fallo", new Exception()));

        Assert.Equal(500, codigo);
    }

    [Fact]
    public void Cancelacion_Se_Clasifica_Como499()
    {
        Assert.Equal(499, ApiExceptionHandler.Clasificar(new OperationCanceledException()).Codigo);
    }

    private sealed class EntornoFalso : IHostEnvironment
    {
        public EntornoFalso(string nombre) => EnvironmentName = nombre;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "AliadaLegal.Web";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
