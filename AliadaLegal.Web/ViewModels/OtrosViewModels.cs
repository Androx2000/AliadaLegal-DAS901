using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AliadaLegal.Web.ViewModels;

public class AliadoFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9+()\-\s]{8,20}$",
        ErrorMessage = "Use entre 8 y 20 caracteres: números, espacios, +, - o paréntesis.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Display(Name = "Tipo de aliado")]
    public TipoAliado TipoAliado { get; set; } = TipoAliado.Abogado;

    [Required(ErrorMessage = "Seleccione el país asignado.")]
    [Display(Name = "País asignado")]
    public int? PaisAsignadoId { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    public IEnumerable<SelectListItem> Paises { get; set; } = new List<SelectListItem>();
}

public class AliadoItemViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string TipoAliado { get; set; } = string.Empty;
    public string PaisAsignado { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class AliadoListaViewModel
{
    public List<AliadoItemViewModel> Aliados { get; set; } = new();
}

public class ConsultaSeguimientoViewModel
{
    [Required(ErrorMessage = "Ingrese el código de seguimiento.")]
    [StringLength(20, MinimumLength = 6, ErrorMessage = "El código tiene entre 6 y 20 caracteres.")]
    [Display(Name = "Código de seguimiento")]
    public string Codigo { get; set; } = string.Empty;

    public bool Buscado { get; set; }
    public TramiteSeguimientoDto? Resultado { get; set; }
}

public class EstadoConteoViewModel
{
    public EstadoTramite Estado { get; set; }
    public int Cantidad { get; set; }
}

public class DashboardViewModel
{
    public string Saludo { get; set; } = string.Empty;
    public bool EsAliado { get; set; }
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Entregados { get; set; }
    public List<EstadoConteoViewModel> PorEstado { get; set; } = new();
    public List<TramiteItemViewModel> Recientes { get; set; } = new();
}
