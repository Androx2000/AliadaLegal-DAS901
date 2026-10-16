using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Tramite
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    [Required]
    public int PaisId { get; set; }

    public Pais Pais { get; set; } = null!;

    [Required]
    public int TipoDocumentoId { get; set; }

    public TipoDocumento TipoDocumento { get; set; } = null!;

    [Required]
    public EstadoTramite Estado { get; set; }
        = EstadoTramite.Solicitado;

    public DateTime FechaSolicitud { get; set; }
        = DateTime.UtcNow;

    [StringLength(1000)]
    public string? Observaciones { get; set; }

    public int? AliadoId { get; set; }

    public Aliado? Aliado { get; set; }
}