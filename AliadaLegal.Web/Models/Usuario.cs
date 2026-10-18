using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public int RolId { get; set; }

    public Rol Rol { get; set; } = null!;

    public int? AliadoId { get; set; }

    public Aliado? Aliado { get; set; }
}