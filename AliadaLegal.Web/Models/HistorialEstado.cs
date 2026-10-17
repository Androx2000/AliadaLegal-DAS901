using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class HistorialEstado
{
    [Key]
    public int Id { get; set; }

    public int TramiteId { get; set; }

    public Tramite Tramite { get; set; } = null!;

    public EstadoTramite? EstadoAnterior { get; set; }

    public EstadoTramite EstadoNuevo { get; set; }

    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(150)]
    public string Responsable { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Comentario { get; set; }
}