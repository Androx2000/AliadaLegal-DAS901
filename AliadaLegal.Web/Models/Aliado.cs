using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Aliado
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
    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public TipoAliado TipoAliado { get; set; }

    public bool Activo { get; set; } = true;

    public int PaisAsignadoId { get; set; }

    public Pais PaisAsignado { get; set; } = null!;

    public ICollection<Tramite> Tramites { get; set; }
        = new List<Tramite>();
}