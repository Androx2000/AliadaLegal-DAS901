namespace AliadaLegal.Web.Repositories;

/// <summary>
/// Contrato CRUD genérico que comparten todos los repositorios.
/// Cada repositorio específico hereda estas operaciones y solo agrega sus consultas propias.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> ObtenerPorIdAsync(int id);
    Task<IReadOnlyList<T>> ListarTodosAsync();
    Task AgregarAsync(T entidad);
    void Actualizar(T entidad);
    void Eliminar(T entidad);
    Task GuardarCambiosAsync();
}