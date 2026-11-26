using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Dtos;

/// <summary>Aliado (abogado o notario) tal como lo devuelve la API.</summary>
public class AliadoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    /// <summary>"Abogado" o "Notario".</summary>
    public string TipoAliado { get; set; } = string.Empty;

    public bool Activo { get; set; }
    public int PaisAsignadoId { get; set; }

    /// <summary>Nombre del país asignado (vacío si no se cargó la relación).</summary>
    public string PaisAsignado { get; set; } = string.Empty;
}

/// <summary>
/// Datos para crear o actualizar un aliado (POST y PUT). No incluye Id ni Tramites
/// para evitar el "overposting".
/// </summary>
public class AliadoGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(254)]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;

    /// <summary>1 = Abogado, 2 = Notario.</summary>
    [EnumDataType(typeof(TipoAliado), ErrorMessage = "Tipo de aliado no válido (1 = Abogado, 2 = Notario).")]
    public TipoAliado TipoAliado { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un país asignado.")]
    public int PaisAsignadoId { get; set; }

    /// <summary>Si se omite, el aliado queda activo.</summary>
    public bool Activo { get; set; } = true;
}

/// <summary>Conversiones entre la entidad Aliado y sus DTOs.</summary>
public static class AliadoMapeo
{
    public static AliadoDto ADto(this Aliado a) => new()
    {
        Id = a.Id,
        Nombre = a.Nombre,
        Correo = a.Correo,
        Telefono = a.Telefono,
        TipoAliado = a.TipoAliado.ToString(),
        Activo = a.Activo,
        PaisAsignadoId = a.PaisAsignadoId,
        PaisAsignado = a.PaisAsignado?.Nombre ?? string.Empty
    };

    /// <summary>Crea la entidad a partir del DTO de entrada; el Id solo se usa al actualizar.</summary>
    public static Aliado AEntidad(this AliadoGuardarDto dto, int id = 0) => new()
    {
        Id = id,
        Nombre = dto.Nombre,
        Correo = dto.Correo,
        Telefono = dto.Telefono,
        TipoAliado = dto.TipoAliado,
        PaisAsignadoId = dto.PaisAsignadoId,
        Activo = dto.Activo
    };
}
