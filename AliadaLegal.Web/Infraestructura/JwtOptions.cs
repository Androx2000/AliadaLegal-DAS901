namespace AliadaLegal.Web.Infraestructura;

/// <summary>Configuración de los tokens JWT (sección "Jwt" de appsettings).</summary>
public class JwtOptions
{
    public string Issuer { get; set; } = "AliadaLegal";
    public string Audience { get; set; } = "AliadaLegal.Api";

    /// <summary>Clave secreta para firmar (HS256), mínimo 32 caracteres. En producción se configura con la variable Jwt__Key.</summary>
    public string Key { get; set; } = string.Empty;

    public int MinutosExpiracion { get; set; } = 60;
}
