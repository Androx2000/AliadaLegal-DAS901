using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.ViewModels;

public class AsignarAliadoViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "El trámite no es válido.")]
    public int TramiteId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Seleccione un aliado válido.")]
    public int AliadoId { get; set; }
}