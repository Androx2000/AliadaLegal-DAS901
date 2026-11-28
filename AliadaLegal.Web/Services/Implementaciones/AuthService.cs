using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace AliadaLegal.Web.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;

    public AuthService(UserManager<Usuario> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string correo, string password)
    {
        // ASP.NET Core Identity: busca por correo y verifica el hash de la contraseña.
        var usuario = await _userManager.FindByEmailAsync(correo.Trim());
        if (usuario == null || !usuario.Activo) return null;

        return await _userManager.CheckPasswordAsync(usuario, password) ? usuario : null;
    }
}
