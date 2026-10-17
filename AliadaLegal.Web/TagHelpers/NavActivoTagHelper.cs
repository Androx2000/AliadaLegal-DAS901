using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AliadaLegal.Web.TagHelpers;

/// <summary>
/// Marca como activo el enlace del menú del controlador actual.
/// Uso: &lt;a class="nav-link" asp-controller="Clientes" asp-action="Index" nav-activo&gt;Clientes&lt;/a&gt;
/// Agrega class="active" y aria-current="page".
/// </summary>
[HtmlTargetElement("a", Attributes = "nav-activo")]
public class NavActivoTagHelper : TagHelper
{
    [HtmlAttributeName("asp-controller")]
    public string? Controlador { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    // Se ejecuta después del AnchorTagHelper para no interferir con el href.
    public override int Order => 1000;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.Attributes.RemoveAll("nav-activo");

        var actual = ViewContext.RouteData.Values["controller"]?.ToString();
        if (Controlador == null || !string.Equals(actual, Controlador, StringComparison.OrdinalIgnoreCase))
            return;

        var clases = output.Attributes.TryGetAttribute("class", out var c) ? c.Value?.ToString() : string.Empty;
        output.Attributes.SetAttribute("class", $"{clases} active".Trim());
        output.Attributes.SetAttribute("aria-current", "page");
    }
}
