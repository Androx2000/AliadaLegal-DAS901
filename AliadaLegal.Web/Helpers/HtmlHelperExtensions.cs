using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AliadaLegal.Web.Helpers;

/// <summary>HTML Helpers propios (sección 5.4): fragmentos reutilizables que se llaman desde Razor con @Html.</summary>
public static class HtmlHelperExtensions
{
    public static IHtmlContent FechaCorta(this IHtmlHelper html, DateTime utc) =>
        new HtmlString(System.Net.WebUtility.HtmlEncode(FechaHelper.Corta(utc)));

    public static IHtmlContent FechaHora(this IHtmlHelper html, DateTime utc) =>
        new HtmlString(System.Net.WebUtility.HtmlEncode(FechaHelper.ConHora(utc)));

    /// <summary>Texto vacío con guion largo, para celdas sin dato.</summary>
    public static IHtmlContent ValorOGuion(this IHtmlHelper html, string? valor) =>
        new HtmlString(string.IsNullOrWhiteSpace(valor)
            ? "<span class=\"text-muted\">—</span>"
            : System.Net.WebUtility.HtmlEncode(valor));

    /// <summary>Etiqueta de campo obligatorio para formularios.</summary>
    public static IHtmlContent MarcaObligatorio(this IHtmlHelper html) =>
        new HtmlString("<span class=\"text-danger\" aria-hidden=\"true\">*</span>");
}
