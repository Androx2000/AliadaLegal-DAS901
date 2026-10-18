using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Cliente
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
    public TipoCliente TipoCliente { get; set; }

    public int PaisOrigenId { get; set; }

    public Pais PaisOrigen { get; set; } = null!;

    [StringLength(30)]
    public string CanalContacto { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }
        = DateTime.UtcNow;

    public ICollection<Tramite> Tramites { get; set; }
        = new List<Tramite>();
}