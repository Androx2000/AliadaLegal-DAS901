using AliadaLegal.Web.Data;
using AliadaLegal.Web.Infraestructura;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---- MVC ----
builder.Services.AddControllersWithViews(opciones =>
{
    // Con tipos anulables habilitados, las propiedades string no anulables serían "obligatorias" de forma implícita.
    // Se desactiva para controlar la obligatoriedad solo con [Required] y mensajes propios.
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// ---- Errores de la API: todas las respuestas de error en formato ProblemDetails (RFC 9457) ----
builder.Services.AddProblemDetails(opciones =>
{
    // Cada error lleva la ruta que lo produjo y un traceId, el mismo que aparece en el log,
    // para que el administrador pueda encontrar el error exacto que vio el cliente.
    opciones.CustomizeProblemDetails = contexto =>
    {
        var http = contexto.HttpContext;
        contexto.ProblemDetails.Instance ??= $"{http.Request.Method} {http.Request.Path}";
        contexto.ProblemDetails.Extensions.TryAdd("traceId",
            System.Diagnostics.Activity.Current?.Id ?? http.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// ---- Swagger / OpenAPI: documentación interactiva de la API REST ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AliadaLegal API",
        Version = "v1",
        Description = "API REST para gestionar clientes, aliados y trámites de AliadaLegal."
    });

    // Botón "Authorize" para probar la API con un token JWT.
    opciones.AgregarSeguridadJwt();

    // Solo se documentan los controladores de la API (los MVC devuelven vistas, no JSON).
    opciones.DocInclusionPredicate((_, api) =>
        api.RelativePath?.StartsWith("api/", StringComparison.OrdinalIgnoreCase) == true);

    // Incluye los comentarios /// <summary> de los controladores y DTOs.
    var xml = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
    opciones.IncludeXmlComments(xml);
});

// ---- Base de datos (SQL Server LocalDB / Azure SQL) ----
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null)));

// ---- Seguridad: ASP.NET Core Identity + cookie (vistas MVC) + JWT (API REST) ----
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    // Sin clave configurada se genera una temporal: los tokens dejan de valer al reiniciar la aplicación.
    // En producción se debe definir la clave (mínimo 32 caracteres) con la variable de entorno Jwt__Key.
    jwt.Key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
}
builder.Services.AddSingleton(jwt);
builder.Services.AddAutenticacionAliadaLegal(jwt);

// ---- Inyección de dependencias (DIP): los controladores solo conocen interfaces ----
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ICodigoSeguimientoGenerador, CodigoSeguimientoGenerador>();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITramiteRepository, TramiteRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IAliadoRepository, AliadoRepository>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IReporteRepository, ReporteRepository>();

builder.Services.AddScoped<ITramiteService, TramiteService>();
builder.Services.AddScoped<ISeguimientoService, SeguimientoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAliadoService, AliadoService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger solo en desarrollo: en producción no se publica el contrato de la API.
    app.UseSwagger();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/swagger/v1/swagger.json", "AliadaLegal API v1");
        opciones.DocumentTitle = "AliadaLegal API";
    });
}
else
{
    app.UseHsts();
}

// ---- Manejo de errores, separado para la API y para las vistas ----
// API: excepciones → ApiExceptionHandler; códigos sin cuerpo (401, 403, 404, 405…) → ProblemDetails en JSON.
app.UseWhen(ApiExceptionHandler.EsRutaApi, api =>
{
    api.UseExceptionHandler();
    api.UseStatusCodePages();
});

// MVC: la página de error amigable de siempre (en desarrollo se ve la página de excepciones detallada).
app.UseWhen(contexto => !ApiExceptionHandler.EsRutaApi(contexto), web =>
{
    if (!app.Environment.IsDevelopment())
        web.UseExceptionHandler("/Home/Error");
    web.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Ruta convencional; el seguimiento público y la API usan rutas por atributos en sus controladores.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Crea la base de datos y carga los datos de demostración al arrancar.
await DbSeeder.InicializarAsync(app.Services);

app.Run();
