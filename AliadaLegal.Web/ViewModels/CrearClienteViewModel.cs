using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.ViewModels;

public class CrearClienteViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, MinimumLength = 3)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30, MinimumLength = 8)]
    public string Telefono { get; set; } = string.Empty;

    [Range(1, 2, ErrorMessage = "Seleccione un tipo de cliente válido.")]
    public TipoCliente TipoCliente { get; set; }
}