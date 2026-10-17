using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Rol
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string NombreRol { get; set; } = string.Empty;

    public ICollection<Usuario> Usuarios { get; set; }
        = new List<Usuario>();
}