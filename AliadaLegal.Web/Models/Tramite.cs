using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Tramite
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    public string CodigoSeguimiento { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int PaisDestinoId { get; set; }
    public Pais PaisDestino { get; set; } = null!;

    public int TipoDocumentoId { get; set; }
    public TipoDocumento TipoDocumento { get; set; } = null!;

    public EstadoTramite Estado { get; set; }
        = EstadoTramite.Solicitado;

    public DateTime FechaSolicitud { get; set; }
        = DateTime.UtcNow;

    public DateTime FechaUltimaActualizacion { get; set; }
        = DateTime.UtcNow;

    [StringLength(1000)]
    public string? Observaciones { get; set; }

    public int? AliadoId { get; set; }
    public Aliado? Aliado { get; set; }

    public Envio? Envio { get; set; }

    public ICollection<HistorialEstado> Historial { get; set; }
        = new List<HistorialEstado>();

    public void AsignarAliado(Aliado aliado)
    {
        ArgumentNullException.ThrowIfNull(aliado);

        Aliado = aliado;
        AliadoId = aliado.Id;
        FechaUltimaActualizacion = DateTime.UtcNow;
    }

    public bool PuedeCambiarA(EstadoTramite nuevo)
    {
        if (Estado == EstadoTramite.Entregado)
            return false;

        return (int)nuevo == (int)Estado + 1 ||
            (Estado == EstadoTramite.DocumentoObtenido &&
             nuevo == EstadoTramite.EnTransito);
    }

    public void ActualizarEstado(
        EstadoTramite nuevo,
        string responsable,
        string? comentario)
    {
        if (!PuedeCambiarA(nuevo))
            throw new InvalidOperationException(
                "La transición de estado no está permitida.");

        var anterior = Estado;
        var ahora = DateTime.UtcNow;

        Estado = nuevo;
        FechaUltimaActualizacion = ahora;

        Historial.Add(new HistorialEstado
        {
            EstadoAnterior = anterior,
            EstadoNuevo = nuevo,
            Fecha = ahora,
            Responsable = responsable,
            Comentario = comentario
        });
    }
}