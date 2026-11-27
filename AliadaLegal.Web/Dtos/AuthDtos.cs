using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Dtos;

/// <summary>Credenciales para obtener un token JWT.</summary>
public class LoginRequestDto
{
    /// <example>admin@aliadalegal.example</example>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    /// <example>Demo1234!</example>
    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Token JWT emitido al iniciar sesión.</summary>
public class TokenResponseDto
{
    /// <summary>Token para enviar en la cabecera: Authorization: Bearer {token}.</summary>
    public string Token { get; set; } = string.Empty;

    public string TipoToken { get; set; } = "Bearer";

    /// <summary>Fecha y hora (UTC) en que el token deja de ser válido.</summary>
    public DateTime ExpiraEn { get; set; }

    public int ExpiraEnSegundos { get; set; }

    public UsuarioActualDto Usuario { get; set; } = new();
}

/// <summary>Datos del usuario autenticado.</summary>
public class UsuarioActualDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;

    /// <summary>Ficha de aliado cuando el rol es Aliado; null en otros roles.</summary>
    public int? AliadoId { get; set; }
}
