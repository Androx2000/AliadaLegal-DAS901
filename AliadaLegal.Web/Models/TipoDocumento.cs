using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class TipoDocumento
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public bool Activo { get; set; } = true;
}