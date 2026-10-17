namespace AliadaLegal.Web.Helpers;

/// <summary>Convierte las fechas guardadas en UTC a la hora de El Salvador para mostrarlas.</summary>
public static class FechaHelper
{
    private static readonly TimeZoneInfo ZonaLocal = ObtenerZona();

    private static TimeZoneInfo ObtenerZona()
    {
        foreach (var id in new[] { "America/El_Salvador", "Central America Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.Utc;
    }

    public static DateTime ALocal(DateTime utc)
    {
        var fecha = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(fecha, ZonaLocal);
    }

    public static string Corta(DateTime utc) => ALocal(utc).ToString("dd/MM/yyyy");

    public static string ConHora(DateTime utc) => ALocal(utc).ToString("dd/MM/yyyy HH:mm");
}
