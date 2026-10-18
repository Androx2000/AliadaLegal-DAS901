using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _clientes;
    private readonly ICatalogoRepository _catalogos;

    public ClienteService(IClienteRepository clientes, ICatalogoRepository catalogos)
    {
        _clientes = clientes;
        _catalogos = catalogos;
    }

    public Task<IReadOnlyList<Cliente>> ListarAsync(string? texto) => _clientes.ListarAsync(texto);

    public Task<Cliente?> ObtenerAsync(int id) => _clientes.ObtenerPorIdAsync(id);

    public async Task<ResultadoOperacion<Cliente>> CrearAsync(Cliente cliente)
    {
        cliente.Correo = cliente.Correo.Trim().ToLowerInvariant();
        cliente.Nombre = cliente.Nombre.Trim();

        if (await _catalogos.ObtenerPaisAsync(cliente.PaisOrigenId) == null)
            return ResultadoOperacion<Cliente>.Fallo("El país de origen seleccionado no existe.");

        if (await _clientes.ExisteCorreoAsync(cliente.Correo, null))
            return ResultadoOperacion<Cliente>.Fallo("Ya existe un cliente registrado con ese correo electrónico.");

        cliente.FechaRegistro = DateTime.UtcNow;
        await _clientes.AgregarAsync(cliente);
        await _clientes.GuardarCambiosAsync();
        return ResultadoOperacion<Cliente>.Ok(cliente);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Cliente datos)
    {
        var existente = await _clientes.ObtenerPorIdAsync(datos.Id);
        if (existente == null)
            return ResultadoOperacion.Fallo("El cliente no existe.");

        var correo = datos.Correo.Trim().ToLowerInvariant();
        if (await _clientes.ExisteCorreoAsync(correo, datos.Id))
            return ResultadoOperacion.Fallo("Otro cliente ya usa ese correo electrónico.");

        existente.Nombre = datos.Nombre.Trim();
        existente.Correo = correo;
        existente.Telefono = datos.Telefono.Trim();
        existente.CanalContacto = datos.CanalContacto;
        existente.TipoCliente = datos.TipoCliente;
        existente.PaisOrigenId = datos.PaisOrigenId;

        await _clientes.GuardarCambiosAsync();
        return ResultadoOperacion.Ok();
    }
}
