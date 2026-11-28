using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AliadaLegal.Web.Infraestructura;

/// <summary>
/// Almacén de ASP.NET Core Identity sobre las tablas existentes (Usuarios y Roles).
/// Permite usar UserManager&lt;Usuario&gt; sin cambiar el esquema de la base de datos.
/// Solo implementa lo necesario para iniciar sesión: buscar por correo, verificar contraseña y leer el rol.
/// </summary>
public class UsuarioStore :
    IUserStore<Usuario>,
    IUserPasswordStore<Usuario>,
    IUserEmailStore<Usuario>,
    IUserRoleStore<Usuario>
{
    private readonly IUsuarioRepository _usuarios;

    public UsuarioStore(IUsuarioRepository usuarios)
    {
        _usuarios = usuarios;
    }

    // ---- IUserStore ----
    public Task<string> GetUserIdAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(user.Id.ToString());

    public Task<string?> GetUserNameAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Correo);

    public Task SetUserNameAsync(Usuario user, string? userName, CancellationToken cancellationToken)
    {
        user.Correo = userName ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetNormalizedUserNameAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Correo.ToUpperInvariant());

    // El nombre normalizado no se guarda: se calcula a partir del correo.
    public Task SetNormalizedUserNameAsync(Usuario user, string? normalizedName, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public async Task<Usuario?> FindByIdAsync(string userId, CancellationToken cancellationToken) =>
        int.TryParse(userId, out var id) ? await _usuarios.ObtenerPorIdAsync(id) : null;

    public Task<Usuario?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) =>
        _usuarios.ObtenerPorCorreoAsync(normalizedUserName);

    // El alta, la edición y la baja de usuarios no se hacen por Identity en este proyecto.
    public Task<IdentityResult> CreateAsync(Usuario user, CancellationToken cancellationToken) => NoSoportado();
    public Task<IdentityResult> DeleteAsync(Usuario user, CancellationToken cancellationToken) => NoSoportado();

    // Identity la llama si el algoritmo de hash se actualiza; aquí no se persiste nada.
    public Task<IdentityResult> UpdateAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(IdentityResult.Success);

    // ---- IUserPasswordStore ----
    public Task SetPasswordHashAsync(Usuario user, string? passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.PasswordHash);

    public Task<bool> HasPasswordAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));

    // ---- IUserEmailStore ----
    public Task SetEmailAsync(Usuario user, string? email, CancellationToken cancellationToken)
    {
        user.Correo = email ?? string.Empty;
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Correo);

    public Task<bool> GetEmailConfirmedAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task SetEmailConfirmedAsync(Usuario user, bool confirmed, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public Task<Usuario?> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        _usuarios.ObtenerPorCorreoAsync(normalizedEmail);

    public Task<string?> GetNormalizedEmailAsync(Usuario user, CancellationToken cancellationToken) =>
        Task.FromResult<string?>(user.Correo.ToUpperInvariant());

    public Task SetNormalizedEmailAsync(Usuario user, string? normalizedEmail, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    // ---- IUserRoleStore (cada usuario tiene un solo rol: Usuario.Rol) ----
    public Task<IList<string>> GetRolesAsync(Usuario user, CancellationToken cancellationToken)
    {
        IList<string> roles = user.Rol is null ? new List<string>() : new List<string> { user.Rol.NombreRol };
        return Task.FromResult(roles);
    }

    public Task<bool> IsInRoleAsync(Usuario user, string roleName, CancellationToken cancellationToken) =>
        Task.FromResult(user.Rol is not null &&
                        string.Equals(user.Rol.NombreRol, roleName, StringComparison.OrdinalIgnoreCase));

    public Task AddToRoleAsync(Usuario user, string roleName, CancellationToken cancellationToken) =>
        throw new NotSupportedException("El rol de un usuario se asigna desde los datos del sistema.");

    public Task RemoveFromRoleAsync(Usuario user, string roleName, CancellationToken cancellationToken) =>
        throw new NotSupportedException("El rol de un usuario se asigna desde los datos del sistema.");

    public Task<IList<Usuario>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public void Dispose()
    {
    }

    private static Task<IdentityResult> NoSoportado() =>
        Task.FromResult(IdentityResult.Failed(new IdentityError
        {
            Code = "NoSoportado",
            Description = "Esta operación no está disponible en el almacén de usuarios."
        }));
}
