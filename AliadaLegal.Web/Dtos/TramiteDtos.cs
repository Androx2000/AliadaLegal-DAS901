using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Dtos;

/// <summary>Trámite tal como aparece en los listados de la API.</summary>
public class TramiteDto
{
    public int Id { get; set; }
    public string CodigoSeguimiento { get; set; } = string.Empty;

    /// <summary>Nombre del estado para usar en código, p. ej. "EnGestion".</summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Nombre del estado para mostrar, p. ej. "En gestión".</summary>
    public string EstadoTexto { get; set; } = string.Empty;

    /// <summary>Porcentaje de avance (0 a 100) según el recorrido del trámite.</summary>
    public int Progreso { get; set; }

    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public int TipoDocumentoId { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public bool RequiereApostilla { get; set; }
    public int PaisDestinoId { get; set; }
    public string PaisDestino { get; set; } = string.Empty;
    public int? AliadoId { get; set; }
    public string? Aliado { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime FechaUltimaActualizacion { get; set; }
}

/// <summary>Detalle de un trámite: además de los datos del listado, su envío y su historial.</summary>
public class TramiteDetalleDto : TramiteDto
{
    /// <summary>Siguiente estado permitido (null si ya terminó).</summary>
    public string? SiguienteEstado { get; set; }

    public EnvioResumenDto? Envio { get; set; }
    public List<HistorialEstadoDto> Historial { get; set; } = new();
}

/// <summary>Un cambio de estado registrado en el historial.</summary>
public class HistorialEstadoDto
{
    public string? EstadoAnterior { get; set; }
    public string EstadoNuevo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? Comentario { get; set; }
}

/// <summary>Datos para crear o editar un trámite (POST y PUT). Solo editable en estado Solicitado.</summary>
public class TramiteGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente.")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un tipo de documento.")]
    public int TipoDocumentoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un país de destino.")]
    public int PaisDestinoId { get; set; }

    [StringLength(1000)]
    public string? Observaciones { get; set; }
}

/// <summary>Aliado que gestionará el trámite (debe estar activo y ser del país de destino).</summary>
public class AsignarAliadoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un aliado.")]
    public int AliadoId { get; set; }
}

/// <summary>Avance de estado. Los estados van en orden; Apostillado solo si el documento la requiere.</summary>
public class CambiarEstadoDto
{
    /// <summary>2 = EnGestion, 3 = DocumentoObtenido, 4 = Apostillado, 6 = Entregado (5 = EnTransito se registra con el envío).</summary>
    [EnumDataType(typeof(EstadoTramite), ErrorMessage = "Estado no válido.")]
    public EstadoTramite NuevoEstado { get; set; }

    [StringLength(1000)]
    public string? Comentario { get; set; }
}

/// <summary>Datos del envío. Registrarlo pasa el trámite a "En tránsito".</summary>
public class RegistrarEnvioDto
{
    [Required(ErrorMessage = "El número de guía es obligatorio.")]
    [StringLength(150)]
    public string NumeroGuia { get; set; } = string.Empty;

    [Required(ErrorMessage = "La empresa de envío es obligatoria.")]
    [StringLength(100)]
    public string EmpresaEnvio { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de despacho es obligatoria.")]
    public DateTime? FechaDespacho { get; set; }
}

/// <summary>Conversiones entre la entidad Tramite y sus DTOs.</summary>
public static class TramiteMapeo
{
    public static TramiteDto ADto(this Tramite t) => Llenar(t, new TramiteDto());

    public static TramiteDetalleDto ADetalleDto(this Tramite t)
    {
        var dto = Llenar(t, new TramiteDetalleDto());
        dto.SiguienteEstado = EstadoTramiteFlujo.Siguiente(t.Estado, dto.RequiereApostilla)?.ToString();
        dto.Envio = t.Envio == null ? null : new EnvioResumenDto
        {
            Id = t.Envio.Id,
            TramiteId = t.Id,
            EmpresaEnvio = t.Envio.EmpresaEnvio,
            NumeroGuia = t.Envio.NumeroGuia,
            FechaDespacho = t.Envio.FechaDespacho,
            FechaEntrega = t.Envio.FechaEntrega
        };
        dto.Historial = t.Historial
            .OrderBy(h => h.Fecha)
            .Select(h => new HistorialEstadoDto
            {
                EstadoAnterior = h.EstadoAnterior?.ToString(),
                EstadoNuevo = h.EstadoNuevo.ToString(),
                Fecha = h.Fecha,
                Responsable = h.Responsable,
                Comentario = h.Comentario
            })
            .ToList();
        return dto;
    }

    private static T Llenar<T>(Tramite t, T dto) where T : TramiteDto
    {
        var requiereApostilla = t.TipoDocumento?.RequiereApostilla ?? false;

        dto.Id = t.Id;
        dto.CodigoSeguimiento = t.CodigoSeguimiento;
        dto.Estado = t.Estado.ToString();
        dto.EstadoTexto = t.Estado.NombreMostrable();
        dto.Progreso = EstadoTramiteFlujo.Progreso(t.Estado, requiereApostilla);
        dto.ClienteId = t.ClienteId;
        dto.Cliente = t.Cliente?.Nombre ?? string.Empty;
        dto.TipoDocumentoId = t.TipoDocumentoId;
        dto.TipoDocumento = t.TipoDocumento?.Nombre ?? string.Empty;
        dto.RequiereApostilla = requiereApostilla;
        dto.PaisDestinoId = t.PaisDestinoId;
        dto.PaisDestino = t.PaisDestino?.Nombre ?? string.Empty;
        dto.AliadoId = t.AliadoId;
        dto.Aliado = t.Aliado?.Nombre;
        dto.Observaciones = t.Observaciones;
        dto.FechaSolicitud = t.FechaSolicitud;
        dto.FechaUltimaActualizacion = t.FechaUltimaActualizacion;
        return dto;
    }
}
