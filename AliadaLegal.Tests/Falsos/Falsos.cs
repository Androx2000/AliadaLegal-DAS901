using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;
using AliadaLegal.Web.Services;

namespace AliadaLegal.Tests.Falsos;

// Implementaciones en memoria de las interfaces (DIP): los servicios se prueban sin base de datos.

public class RelojFijo : TimeProvider
{
    private readonly DateTimeOffset _ahora;
    public RelojFijo(int anio = 2026, int mes = 11, int dia = 14) =>
        _ahora = new DateTimeOffset(anio, mes, dia, 10, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _ahora;
}

public class GeneradorFijo : ICodigoSeguimientoGenerador
{
    private readonly Queue<string> _codigos;
    public int Llamadas { get; private set; }
    public GeneradorFijo(params string[] codigos) => _codigos = new Queue<string>(codigos);
    public string Generar()
    {
        Llamadas++;
        return _codigos.Dequeue();
    }
}

public class TramiteRepositorioFalso : ITramiteRepository
{
    public List<Tramite> Datos { get; } = new();
    public int Guardados { get; private set; }

    public Task<IReadOnlyList<Tramite>> BuscarAsync(TramiteFiltro filtro, int? limite = null)
    {
        IEnumerable<Tramite> q = Datos;
        if (filtro.Estado.HasValue) q = q.Where(t => t.Estado == filtro.Estado);
        if (filtro.AliadoId.HasValue) q = q.Where(t => t.AliadoId == filtro.AliadoId);
        if (filtro.ClienteId.HasValue) q = q.Where(t => t.ClienteId == filtro.ClienteId);
        if (limite.HasValue) q = q.Take(limite.Value);
        return Task.FromResult<IReadOnlyList<Tramite>>(q.ToList());
    }

    public Task<Tramite?> ObtenerPorIdAsync(int id) => Task.FromResult(Datos.FirstOrDefault(t => t.Id == id));

    public Task<Tramite?> ObtenerPorCodigoAsync(string codigo) =>
        Task.FromResult(Datos.FirstOrDefault(t => t.CodigoSeguimiento == codigo));

    public Task<bool> CodigoExisteAsync(string codigo) =>
        Task.FromResult(Datos.Any(t => t.CodigoSeguimiento == codigo));

    public Task<Dictionary<EstadoTramite, int>> ContarPorEstadoAsync(int? aliadoId) =>
        Task.FromResult(Datos
            .Where(t => !aliadoId.HasValue || t.AliadoId == aliadoId)
            .GroupBy(t => t.Estado)
            .ToDictionary(g => g.Key, g => g.Count()));

    public Task AgregarAsync(Tramite tramite)
    {
        tramite.Id = Datos.Count + 1;
        Datos.Add(tramite);
        return Task.CompletedTask;
    }

    public Task GuardarCambiosAsync()
    {
        Guardados++;
        return Task.CompletedTask;
    }
}

public class ClienteRepositorioFalso : IClienteRepository
{
    public List<Cliente> Datos { get; } = new();
    public int Guardados { get; private set; }

    public Task<IReadOnlyList<Cliente>> ListarAsync(string? texto) =>
        Task.FromResult<IReadOnlyList<Cliente>>(Datos.ToList());

    public Task<Cliente?> ObtenerPorIdAsync(int id) => Task.FromResult(Datos.FirstOrDefault(c => c.Id == id));

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId) =>
        Task.FromResult(Datos.Any(c => c.Correo == correo && c.Id != excluirId));

    public Task AgregarAsync(Cliente cliente)
    {
        cliente.Id = Datos.Count + 1;
        Datos.Add(cliente);
        return Task.CompletedTask;
    }

    // Miembros que la rama de CRUD de clientes agrega a IClienteRepository. Aquí son públicos y no
    // dependen de la interfaz, así el falso compila con o sin esos cambios.
    public bool ConTramites { get; set; }
    public Task<bool> TieneTramitesAsync(int clienteId) => Task.FromResult(ConTramites);
    public void Eliminar(Cliente cliente) => Datos.Remove(cliente);

    public Task GuardarCambiosAsync()
    {
        Guardados++;
        return Task.CompletedTask;
    }
}

public class AliadoRepositorioFalso : IAliadoRepository
{
    public List<Aliado> Datos { get; } = new();
    public int TramitesAbiertos { get; set; }

    public Task<IReadOnlyList<Aliado>> ListarAsync(bool soloActivos) =>
        Task.FromResult<IReadOnlyList<Aliado>>(Datos.Where(a => !soloActivos || a.Activo).ToList());

    public Task<IReadOnlyList<Aliado>> ListarPorPaisAsync(int paisId) =>
        Task.FromResult<IReadOnlyList<Aliado>>(Datos.Where(a => a.PaisAsignadoId == paisId).ToList());

    public Task<Aliado?> ObtenerPorIdAsync(int id) => Task.FromResult(Datos.FirstOrDefault(a => a.Id == id));

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId) =>
        Task.FromResult(Datos.Any(a => a.Correo == correo && a.Id != excluirId));

    public Task AgregarAsync(Aliado aliado)
    {
        aliado.Id = Datos.Count + 1;
        Datos.Add(aliado);
        return Task.CompletedTask;
    }

    public Task<int> ContarTramitesAbiertosAsync(int aliadoId) => Task.FromResult(TramitesAbiertos);

    public Task GuardarCambiosAsync() => Task.CompletedTask;
}

public class CatalogoRepositorioFalso : ICatalogoRepository
{
    public List<Pais> Paises { get; } = new()
    {
        new Pais { Id = 1, Nombre = "El Salvador", CodigoIso = "SV" },
        new Pais { Id = 2, Nombre = "Guatemala", CodigoIso = "GT" }
    };

    public List<TipoDocumento> Tipos { get; } = new()
    {
        new TipoDocumento { Id = 1, Nombre = "Partida de nacimiento", RequiereApostilla = false },
        new TipoDocumento { Id = 2, Nombre = "Antecedentes penales", RequiereApostilla = true }
    };

    public Task<IReadOnlyList<Pais>> ListarPaisesAsync() => Task.FromResult<IReadOnlyList<Pais>>(Paises);
    public Task<IReadOnlyList<TipoDocumento>> ListarTiposDocumentoAsync() => Task.FromResult<IReadOnlyList<TipoDocumento>>(Tipos);
    public Task<Pais?> ObtenerPaisAsync(int id) => Task.FromResult(Paises.FirstOrDefault(p => p.Id == id));
    public Task<TipoDocumento?> ObtenerTipoDocumentoAsync(int id) => Task.FromResult(Tipos.FirstOrDefault(t => t.Id == id));
}
