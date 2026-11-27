using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AliadaLegal.Web.Controllers.Api;

/// <summary>
/// Autenticación de la API: se inicia sesión con correo y contraseña y se recibe un token JWT
/// que se envía en las demás peticiones: Authorization: Bearer {token}.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthApiController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly IJwtTokenService _tokens;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(IAuthService auth, IJwtTokenService tokens, ILogger<AuthApiController> logger)
    {
        _auth = auth;
        _tokens = tokens;
        _logger = logger;
    }

    /// <summary>Inicia sesión y devuelve un token JWT.</summary>
    /// <response code="200">Credenciales correctas: token y datos del usuario.</response>
    /// <response code="400">Faltan datos o el correo no tiene formato válido.</response>
    /// <response code="401">Correo o contraseña incorrectos, o usuario dado de baja.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginRequestDto solicitud)
    {
        var usuario = await _auth.ValidarCredencialesAsync(solicitud.Correo, solicitud.Password);
        if (usuario == null)
        {
            // Mensaje genérico: no revela si el correo existe.
            _logger.LogWarning("Intento de inicio de sesión fallido en la API para {Correo}", solicitud.Correo);
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciales inválidas",
                detail: "Correo o contraseña incorrectos.");
        }

        _logger.LogInformation("Inicio de sesión en la API: usuario {UsuarioId}", usuario.Id);
        return Ok(_tokens.GenerarToken(usuario));
    }

    /// <summary>Devuelve los datos del usuario del token (sirve para comprobar que el token es válido).</summary>
    /// <response code="200">Usuario autenticado.</response>
    /// <response code="401">Falta el token o es inválido o está vencido.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UsuarioActualDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<UsuarioActualDto> Yo() =>
        Ok(new UsuarioActualDto
        {
            Id = User.ObtenerId(),
            Nombre = User.ObtenerNombre(),
            Correo = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Rol = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
            AliadoId = User.ObtenerAliadoId()
        });
}
