using System.ComponentModel.DataAnnotations;
using System.Reflection;
using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Helpers;

public static class EnumExtensions
{
    /// <summary>Devuelve el texto del atributo [Display(Name)] o el nombre del miembro si no lo tiene.</summary>
    public static string NombreMostrable(this Enum valor)
    {
        var miembro = valor.GetType().GetMember(valor.ToString()).FirstOrDefault();
        var display = miembro?.GetCustomAttribute<DisplayAttribute>();
        return display?.GetName() ?? valor.ToString();
    }

    /// <summary>Clave en minúsculas usada como clase CSS: estado-entransito, estado-entregado...</summary>
    public static string ClaveCss(this EstadoTramite estado) => estado.ToString().ToLowerInvariant();
}
