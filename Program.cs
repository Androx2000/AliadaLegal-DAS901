using AliadaLegal.Web.Data;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---- MVC ----
builder.Services.AddControllersWithViews(opciones =>
{
    // Con tipos anulables habilitados, las propiedades string no anulables serían "obligatorias" de forma implícita.
    // Se desactiva para controlar la obligatoriedad solo con [Required] y mensajes propios.
    opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// ---- Base de datos (SQL Server LocalDB) ----
builder.Services.AddDbContext<AppDbContext>(opciones =>
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---- Autenticación por cookie y autorización por roles (RNF01, RNF02) ----
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Account/Login";
        opciones.AccessDeniedPath = "/Account/AccessDenied";
        opciones.Cookie.Name = "AliadaLegal.Auth";
        opciones.Cookie.HttpOnly = true;
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// ---- Inyección de dependencias (DIP): los controladores solo conocen interfaces ----
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services.AddScoped<ITramiteRepository, TramiteRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IAliadoRepository, AliadoRepository>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<ITramiteService, TramiteService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAliadoService, AliadoService>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");
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
