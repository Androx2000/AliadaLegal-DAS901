using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Envio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int TramiteId { get; set; }

    public Tramite Tramite { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string EmpresaEnvio { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string NumeroGuia { get; set; } = string.Empty;

    public DateTime FechaDespacho { get; set; }
        = DateTime.UtcNow;

    public DateTime? FechaEntrega { get; set; }
}