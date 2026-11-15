namespace AliadaLegal.Web.Services;

/// <summary>
/// Genera códigos de seguimiento del tipo AL-2026-K7M2QX.
/// Vive aparte de TramiteService (SRP) y se puede reemplazar en las pruebas (DIP).
/// </summary>
public interface ICodigoSeguimientoGenerador
{
    string Generar();
}
