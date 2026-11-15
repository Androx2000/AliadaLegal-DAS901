using AliadaLegal.Web.Dtos;

namespace AliadaLegal.Web.Services;

/// <summary>
/// Consulta pública del estado de un trámite (RF06). Interfaz propia (ISP): la vista pública y la API
/// solo necesitan esta operación y no tienen por qué depender de crear, asignar o cambiar estados.
/// </summary>
public interface ISeguimientoService
{
    Task<TramiteSeguimientoDto?> ConsultarAsync(string codigo);
}
