using System.Text;
using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AliadaLegal.Web.Infraestructura;

/// <summary>
/// Configuración de seguridad: ASP.NET Core Identity (UserManager) y dos formas de autenticarse:
/// cookie para las vistas MVC y token JWT (Bearer) para la API REST.
/// </summary>
public static class AutenticacionExtensions
{
    private const string EsquemaCookieOJwt = "CookieOJwt";

    public static IServiceCollection AddAutenticacionAliadaLegal(this IServiceCollection servicios, JwtOptions jwt)
    {
        // Identity (núcleo): UserManager<Usuario> sobre las tablas existentes, mediante UsuarioStore.
        servicios.AddIdentityCore<Usuario>().AddUserStore<UsuarioStore>();

        servicios
            .AddAuthentication(opciones =>
            {
                // Esquema "inteligente": si la petición trae "Authorization: Bearer ..." se valida el JWT;
                // si no, se usa la cookie (comportamiento de siempre en las vistas MVC).
                opciones.DefaultScheme = EsquemaCookieOJwt;
            })
            .AddPolicyScheme(EsquemaCookieOJwt, "Cookie o JWT", opciones =>
            {
                opciones.ForwardDefaultSelector = contexto =>
                {
                    var cabecera = contexto.Request.Headers.Authorization.ToString();
                    return cabecera.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : CookieAuthenticationDefaults.AuthenticationScheme;
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opciones =>
            {
                opciones.LoginPath = "/Account/Login";
                opciones.AccessDeniedPath = "/Account/AccessDenied";
                opciones.Cookie.Name = "AliadaLegal.Auth";
                opciones.Cookie.HttpOnly = true;
                opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
                opciones.SlidingExpiration = true;

                // Las rutas /api no deben redirigir a la página de login (302 + HTML):
                // un cliente de API espera 401 (sin sesión) o 403 (sin permiso).
                opciones.Events.OnRedirectToLogin = contexto =>
                {
                    if (contexto.Request.Path.StartsWithSegments("/api"))
                        contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    else
                        contexto.Response.Redirect(contexto.RedirectUri);
                    return Task.CompletedTask;
                };
                opciones.Events.OnRedirectToAccessDenied = contexto =>
                {
                    if (contexto.Request.Path.StartsWithSegments("/api"))
                        contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                    else
                        contexto.Response.Redirect(contexto.RedirectUri);
                    return Task.CompletedTask;
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opciones =>
            {
                // Los claims del token usan los mismos tipos que la cookie (ClaimTypes.*), sin traducirlos.
                opciones.MapInboundClaims = false;
                opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = System.Security.Claims.ClaimTypes.Name,
                    RoleClaimType = System.Security.Claims.ClaimTypes.Role
                };
            });

        servicios.AddAuthorization();
        return servicios;
    }

    /// <summary>Agrega el botón "Authorize" de Swagger para probar la API con un token Bearer.</summary>
    public static void AgregarSeguridadJwt(this SwaggerGenOptions opciones)
    {
        opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Pegue solo el token que devuelve POST /api/auth/login (sin la palabra Bearer)."
        });

        opciones.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                },
                Array.Empty<string>()
            }
        });
    }
}
