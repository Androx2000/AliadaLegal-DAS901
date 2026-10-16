using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Aliado
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Telefono { get; set; }

    [Required]
    public TipoAliado Tipo { get; set; }

    public bool Activo { get; set; } = true;

    public int PaisId { get; set; }

    public Pais Pais { get; set; } = null!;

    public ICollection<Tramite> Tramites { get; set; }
        = new List<Tramite>();
}