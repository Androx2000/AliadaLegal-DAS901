using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Infraestructura;

/// <summary>
/// Manejador global de excepciones no controladas en las rutas /api.
/// Convierte cada excepción en una respuesta ProblemDetails (RFC 9457) con el código HTTP
/// que le corresponde, la registra en el log con el nivel adecuado y nunca expone
/// la traza interna fuera del entorno de desarrollo.
/// Las rutas MVC siguen usando la página /Home/Error.
/// </summary>
public class ApiExceptionHandler : IExceptionHandler
{
    // Números de error de SQL Server para filas duplicadas en un índice único o clave primaria.
    private const int SqlIndiceUnicoDuplicado = 2601;
    private const int SqlClaveDuplicada = 2627;

    private readonly IProblemDetailsService _problemas;
    private readonly IHostEnvironment _entorno;
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(IProblemDetailsService problemas, IHostEnvironment entorno,
        ILogger<ApiExceptionHandler> logger)
    {
        _problemas = problemas;
        _entorno = entorno;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion,
        CancellationToken cancelacion)
    {
        if (!EsRutaApi(contexto))
            return false; // que lo atienda el manejador del MVC

        var (codigo, titulo, detalle) = Clasificar(excepcion);
        var ruta = $"{contexto.Request.Method} {contexto.Request.Path}";

        // El middleware de excepciones ya registró la excepción completa (nivel Error, con traza).
        // Aquí solo se deja constancia de cómo se clasificó cuando no es un error del servidor.
        if (codigo < StatusCodes.Status500InternalServerError)
            _logger.LogWarning("{Ruta} respondió {Codigo}: {Titulo} ({TipoExcepcion})",
                ruta, codigo, titulo, excepcion.GetType().Name);

        // En desarrollo se muestra el mensaje real para depurar; en producción, un texto genérico.
        if (_entorno.IsDevelopment() && codigo >= StatusCodes.Status500InternalServerError)
            detalle = excepcion.Message;

        contexto.Response.StatusCode = codigo;
        return await _problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            Exception = excepcion,
            ProblemDetails = new ProblemDetails { Status = codigo, Title = titulo, Detail = detalle }
        });
    }

    /// <summary>Decide el código HTTP, el título y el detalle para cada tipo de excepción.</summary>
    public static (int Codigo, string Titulo, string Detalle) Clasificar(Exception excepcion) => excepcion switch
    {
        // Dos peticiones guardan el mismo correo a la vez: las dos pasan ExisteCorreoAsync,
        // pero el índice único de la base rechaza la segunda.
        DbUpdateException { InnerException: SqlException { Number: SqlIndiceUnicoDuplicado or SqlClaveDuplicada } } =>
            (StatusCodes.Status409Conflict, "Registro duplicado.",
                "Ya existe un registro con ese valor único (por ejemplo, el correo electrónico)."),

        DbUpdateConcurrencyException =>
            (StatusCodes.Status409Conflict, "Conflicto de concurrencia.",
                "Otro usuario modificó este registro al mismo tiempo. Vuelva a consultarlo e intente de nuevo."),

        // El cliente HTTP canceló la petición antes de que terminara.
        OperationCanceledException =>
            (499, "Petición cancelada.", "El cliente canceló la petición."),

        _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor.",
                "Ocurrió un error inesperado. Si persiste, informe el traceId al administrador.")
    };

    public static bool EsRutaApi(HttpContext contexto) =>
        contexto.Request.Path.StartsWithSegments("/api");
}
