using System.ComponentModel.DataAnnotations;
using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Dtos;

/// <summary>
/// Cliente tal como lo devuelve la API. No expone la entidad de EF:
/// evita ciclos de serialización (Cliente → Tramites → Cliente) y campos internos.
/// </summary>
public class ClienteDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;

    /// <summary>"Individual" o "Bufete".</summary>
    public string TipoCliente { get; set; } = string.Empty;

    public int PaisOrigenId { get; set; }

    /// <summary>Nombre del país de origen (vacío si no se cargó la relación).</summary>
    public string PaisOrigen { get; set; } = string.Empty;

    public string CanalContacto { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public int CantidadTramites { get; set; }
}

/// <summary>
/// Datos que el cliente de la API envía para crear o actualizar (POST y PUT).
/// Solo trae los campos editables: Id, FechaRegistro y Tramites no se pueden
/// asignar desde afuera (evita el "overposting").
/// </summary>
public class ClienteGuardarDto
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

    /// <summary>1 = Individual, 2 = Bufete.</summary>
    [EnumDataType(typeof(TipoCliente), ErrorMessage = "Tipo de cliente no válido (1 = Individual, 2 = Bufete).")]
    public TipoCliente TipoCliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un país de origen.")]
    public int PaisOrigenId { get; set; }

    [StringLength(30)]
    public string CanalContacto { get; set; } = string.Empty;
}

/// <summary>Conversiones entre la entidad Cliente y sus DTOs.</summary>
public static class ClienteMapeo
{
    public static ClienteDto ADto(this Cliente c) => new()
    {
        Id = c.Id,
        Nombre = c.Nombre,
        Correo = c.Correo,
        Telefono = c.Telefono,
        TipoCliente = c.TipoCliente.ToString(),
        PaisOrigenId = c.PaisOrigenId,
        PaisOrigen = c.PaisOrigen?.Nombre ?? string.Empty,
        CanalContacto = c.CanalContacto,
        FechaRegistro = c.FechaRegistro,
        CantidadTramites = c.Tramites?.Count ?? 0
    };

    /// <summary>Crea la entidad a partir del DTO de entrada; el Id solo se usa al actualizar.</summary>
    public static Cliente AEntidad(this ClienteGuardarDto dto, int id = 0) => new()
    {
        Id = id,
        Nombre = dto.Nombre,
        Correo = dto.Correo,
        Telefono = dto.Telefono,
        TipoCliente = dto.TipoCliente,
        PaisOrigenId = dto.PaisOrigenId,
        CanalContacto = dto.CanalContacto
    };
}
