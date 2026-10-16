using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.ViewModels;

public class CrearTramiteViewModel

{
    
    [Range(1, int.MaxValue,
    ErrorMessage ="Seleccione un cliente valido.")]
    public int ClienteId {get; set; }
    
    [Range(1, int.MaxValue,
    ErrorMessage ="Seleccione un pais valido.")]
    public int PaisId { get; set; }

    [Range(1, int.MaxValue,
    ErrorMessage ="Seleccione un documento valido.")]
    public int TipoDocumentoId { get; set; }

    [StringLength(1000)]
    public string? Observaciones { get; set; }
}