using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AliadaLegal.Web.TagHelpers;

/// <summary>
/// Insignia de color para el estado de un trámite.
/// Uso con el enum:   &lt;estado-badge estado="@item.Estado" /&gt;
/// Uso con texto:     &lt;estado-badge texto="@dto.Estado" clave="@dto.EstadoClave" /&gt;
/// Siempre muestra texto además del color (accesible para daltonismo).
/// </summary>
[HtmlTargetElement("estado-badge", TagStructure = TagStructure.WithoutEndTag)]
public class EstadoBadgeTagHelper : TagHelper
{
    /// <summary>Estado como enum (vistas internas).</summary>
    public EstadoTramite? Estado { get; set; }

    /// <summary>Texto ya formateado (consulta pública, DTO).</summary>
    public string? Texto { get; set; }

    /// <summary>Clave CSS del estado, p. ej. "entransito".</summary>
    public string? Clave { get; set; }

    /// <summary>Muestra u oculta el ícono. Por defecto true.</summary>
    public bool Icono { get; set; } = true;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        string texto, clase, icono;

        if (Estado.HasValue)
        {
            texto = EstadoTramiteUi.Etiqueta(Estado.Value);
            clase = EstadoTramiteUi.ClaseCss(Estado.Value);
            icono = EstadoTramiteUi.Icono(Estado.Value);
        }
        else
        {
            texto = Texto ?? "—";
            clase = EstadoTramiteUi.ClaseCss(Clave ?? Texto);
            icono = EstadoTramiteUi.Icono(Clave ?? Texto);
        }

        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;

        var claseExtra = output.Attributes.TryGetAttribute("class", out var c) ? " " + c.Value : string.Empty;
        output.Attributes.SetAttribute("class", $"estado-badge {clase}{claseExtra}");

        if (Icono)
            output.Content.AppendHtml($"<i class=\"bi {icono}\" aria-hidden=\"true\"></i>");
        output.Content.Append(texto);
    }
}
