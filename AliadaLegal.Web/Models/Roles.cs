namespace AliadaLegal.Web.Models;

/// <summary>Nombres de rol usados en [Authorize(Roles = ...)] y en las vistas.</summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Asesor = "Asesor";
    public const string Aliado = "Aliado";

    public const string AdministradorOAsesor = Administrador + "," + Asesor;
}
