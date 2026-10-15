using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Correo no válido.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public TipoCliente TipoCliente { get; set; }

    public DateTime FechaRegistro { get; set; }
        = DateTime.UtcNow;

    public ICollection<Tramite> Tramites { get; set; }
        = new List<Tramite>();
}