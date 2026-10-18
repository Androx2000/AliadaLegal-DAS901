using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AliadaLegal.Web.ViewModels;

/// <summary>Fila de trámite para tablas y tarjetas.</summary>
public class TramiteItemViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string PaisDestino { get; set; } = string.Empty;
    public EstadoTramite Estado { get; set; }
    public string? Aliado { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime UltimaActualizacion { get; set; }
}

public class TramiteListaViewModel
{
    public string? Texto { get; set; }
    public EstadoTramite? Estado { get; set; }
    public bool EsAliado { get; set; }
    public List<TramiteItemViewModel> Tramites { get; set; } = new();
}

public class TramiteFormViewModel
{
    [Required(ErrorMessage = "Seleccione el cliente.")]
    [Display(Name = "Cliente")]
    public int? ClienteId { get; set; }

    [Required(ErrorMessage = "Seleccione el tipo de documento.")]
    [Display(Name = "Tipo de documento")]
    public int? TipoDocumentoId { get; set; }

    [Required(ErrorMessage = "Seleccione el país donde se gestionará el trámite.")]
    [Display(Name = "País del trámite")]
    public int? PaisDestinoId { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    public IEnumerable<SelectListItem> Clientes { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> TiposDocumento { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Paises { get; set; } = new List<SelectListItem>();
}

public class AsignarAliadoViewModel
{
    public int TramiteId { get; set; }

    [Required(ErrorMessage = "Seleccione un aliado.")]
    [Display(Name = "Aliado")]
    public int? AliadoId { get; set; }

    /// <summary>Aliados activos del país del trámite.</summary>
    public IEnumerable<SelectListItem> Aliados { get; set; } = new List<SelectListItem>();
}

public class CambiarEstadoViewModel
{
    public int TramiteId { get; set; }
    public EstadoTramite NuevoEstado { get; set; }

    [StringLength(300, ErrorMessage = "El comentario no puede superar 300 caracteres.")]
    [Display(Name = "Comentario (opcional)")]
    public string? Comentario { get; set; }
}

public class EnvioFormViewModel
{
    public int TramiteId { get; set; }

    [Required(ErrorMessage = "Ingrese el número de guía o rastreo.")]
    [StringLength(60, ErrorMessage = "La guía no puede superar 60 caracteres.")]
    [Display(Name = "Número de guía")]
    public string NumeroGuia { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese la empresa de envío.")]
    [StringLength(80, ErrorMessage = "El nombre de la empresa no puede superar 80 caracteres.")]
    [Display(Name = "Empresa de envío")]
    public string EmpresaEnvio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indique la fecha de despacho.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de despacho")]
    public DateTime? FechaDespacho { get; set; }
}

public class HistorialItemViewModel
{
    public EstadoTramite Estado { get; set; }
    public DateTime Fecha { get; set; }
    public string Responsable { get; set; } = string.Empty;
    public string? Comentario { get; set; }
}

public class TramiteDetalleViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public EstadoTramite Estado { get; set; }
    public int Progreso { get; set; }
    public string EnlacePublico { get; set; } = string.Empty;

    public int ClienteId { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string ClienteCorreo { get; set; } = string.Empty;
    public string ClienteContacto { get; set; } = string.Empty;

    public string TipoDocumento { get; set; } = string.Empty;
    public bool RequiereApostilla { get; set; }
    public string PaisDestino { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public DateTime UltimaActualizacion { get; set; }

    public string? Aliado { get; set; }
    public string? AliadoContacto { get; set; }

    public string? EnvioGuia { get; set; }
    public string? EnvioEmpresa { get; set; }
    public DateTime? EnvioFecha { get; set; }

    public List<HistorialItemViewModel> Historial { get; set; } = new();

    // Qué acciones se ofrecen según el estado y el rol (la autorización real se valida en el servidor).
    public EstadoTramite? EstadoSiguiente { get; set; }
    public bool PuedeAsignarAliado { get; set; }
    public bool PuedeAvanzar { get; set; }
    public bool PuedeRegistrarEnvio { get; set; }

    public AsignarAliadoViewModel Asignar { get; set; } = new();
    public CambiarEstadoViewModel CambiarEstado { get; set; } = new();
    public EnvioFormViewModel EnvioForm { get; set; } = new();
}
