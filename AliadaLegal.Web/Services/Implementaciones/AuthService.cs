using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using Microsoft.AspNetCore.Identity;

namespace AliadaLegal.Web.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher<Usuario> _hasher;

    public AuthService(IUsuarioRepository usuarios, IPasswordHasher<Usuario> hasher)
    {
        _usuarios = usuarios;
        _hasher = hasher;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string correo, string password)
    {
        var usuario = await _usuarios.ObtenerPorCorreoAsync(correo.Trim());
        if (usuario == null || !usuario.Activo) return null;

        var resultado = _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, password);
        return resultado == PasswordVerificationResult.Failed ? null : usuario;
    }
}
