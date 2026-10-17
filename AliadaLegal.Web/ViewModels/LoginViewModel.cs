using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Ingrese su correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese su contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener la sesión iniciada")]
    public bool Recordarme { get; set; }

    public string? ReturnUrl { get; set; }
}
