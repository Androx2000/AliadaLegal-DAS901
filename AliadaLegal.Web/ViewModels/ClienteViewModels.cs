using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AliadaLegal.Web.ViewModels;

public class ClienteFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
    [Display(Name = "Nombre completo o razón social")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9+()\-\s]{8,20}$",
        ErrorMessage = "Use entre 8 y 20 caracteres: números, espacios, +, - o paréntesis.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione el canal de contacto preferido.")]
    [RegularExpression("^(WhatsApp|Correo|Llamada|SMS)$", ErrorMessage = "Seleccione un canal de contacto válido.")]
    [Display(Name = "Canal de contacto preferido")]
    public string CanalContacto { get; set; } = "WhatsApp";

    [Display(Name = "Tipo de cliente")]
    public TipoCliente TipoCliente { get; set; } = TipoCliente.Individual;

    [Required(ErrorMessage = "Seleccione el país de origen.")]
    [Display(Name = "País de origen")]
    public int? PaisOrigenId { get; set; }

    public IEnumerable<SelectListItem> Paises { get; set; } = new List<SelectListItem>();
}

public class ClienteItemViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = string.Empty;
    public bool EsBufete { get; set; }
    public string PaisOrigen { get; set; } = string.Empty;
    public int TotalTramites { get; set; }
}

public class ClienteListaViewModel
{
    public string? Texto { get; set; }
    public List<ClienteItemViewModel> Clientes { get; set; } = new();
}

public class ClienteDetalleViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string CanalContacto { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = string.Empty;
    public string PaisOrigen { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public List<TramiteItemViewModel> Tramites { get; set; } = new();
}
