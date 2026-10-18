using System.Security.Cryptography;
using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public class TramiteService : ITramiteService
{
    // Sin 0/O ni 1/I para evitar errores al dictar el código por teléfono o WhatsApp.
    private const string AlfabetoCodigo = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private readonly ITramiteRepository _tramites;
    private readonly IClienteRepository _clientes;
    private readonly IAliadoRepository _aliados;
    private readonly ICatalogoRepository _catalogos;

    public TramiteService(
        ITramiteRepository tramites,
        IClienteRepository clientes,
        IAliadoRepository aliados,
        ICatalogoRepository catalogos)
    {
        _tramites = tramites;
        _clientes = clientes;
        _aliados = aliados;
        _catalogos = catalogos;
    }

    // ---------------------------------------------------------------- RF02
    public async Task<ResultadoOperacion<Tramite>> CrearAsync(int clienteId, int tipoDocumentoId,
        int paisDestinoId, string? observaciones, string responsable)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(clienteId);
        if (cliente == null)
            return ResultadoOperacion<Tramite>.Fallo("El cliente seleccionado no existe.");

        var tipo = await _catalogos.ObtenerTipoDocumentoAsync(tipoDocumentoId);
        if (tipo == null)
            return ResultadoOperacion<Tramite>.Fallo("El tipo de documento seleccionado no existe.");

        var pais = await _catalogos.ObtenerPaisAsync(paisDestinoId);
        if (pais == null)
            return ResultadoOperacion<Tramite>.Fallo("El país seleccionado no existe.");

        var ahora = DateTime.UtcNow;
        var tramite = new Tramite
        {
            CodigoSeguimiento = await GenerarCodigoUnicoAsync(),
            ClienteId = cliente.Id,
            TipoDocumentoId = tipo.Id,
            PaisDestinoId = pais.Id,
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim(),
            Estado = EstadoTramite.Solicitado,
            FechaSolicitud = ahora,
            FechaUltimaActualizacion = ahora
        };

        tramite.Historial.Add(new HistorialEstado
        {
            EstadoAnterior = null,
            EstadoNuevo = EstadoTramite.Solicitado,
            Fecha = ahora,
            Responsable = responsable,
            Comentario = "Solicitud registrada."
        });

        await _tramites.AgregarAsync(tramite);
        await _tramites.GuardarCambiosAsync();
        return ResultadoOperacion<Tramite>.Ok(tramite);
    }

    // ---------------------------------------------------------------- RF03
    public async Task<ResultadoOperacion> AsignarAliadoAsync(int tramiteId, int aliadoId, string responsable)
    {
        var tramite = await _tramites.ObtenerPorIdAsync(tramiteId);
        if (tramite == null)
            return ResultadoOperacion.Fallo("El trámite no existe.");

        if (tramite.Estado == EstadoTramite.Entregado)
            return ResultadoOperacion.Fallo("El trámite ya fue entregado y no puede reasignarse.");

        var aliado = await _aliados.ObtenerPorIdAsync(aliadoId);
        if (aliado == null || !aliado.Activo)
            return ResultadoOperacion.Fallo("El aliado seleccionado no existe o está inactivo.");

        if (aliado.PaisAsignadoId != tramite.PaisDestinoId)
            return ResultadoOperacion.Fallo("El aliado debe pertenecer al país en el que se gestiona el trámite.");

        tramite.AsignarAliado(aliado);

        // Al asignar un aliado por primera vez, la gestión comienza.
        if (tramite.Estado == EstadoTramite.Solicitado)
            tramite.ActualizarEstado(EstadoTramite.EnGestion, responsable, $"Asignado a {aliado.Nombre}.");

        await _tramites.GuardarCambiosAsync();
        return ResultadoOperacion.Ok();
    }

    // ---------------------------------------------------------------- RF04
    public async Task<ResultadoOperacion> CambiarEstadoAsync(int tramiteId, EstadoTramite nuevo,
        string? comentario, string responsable, int? aliadoIdRestriccion)
    {
        var tramite = await _tramites.ObtenerPorIdAsync(tramiteId);
        if (tramite == null)
            return ResultadoOperacion.Fallo("El trámite no existe.");

        if (aliadoIdRestriccion.HasValue && tramite.AliadoId != aliadoIdRestriccion)
            return ResultadoOperacion.Fallo("Solo puede actualizar los trámites que tiene asignados.");

        if (!tramite.PuedeCambiarA(nuevo))
            return ResultadoOperacion.Fallo(
                $"No se puede pasar de «{tramite.Estado.NombreMostrable()}» a «{nuevo.NombreMostrable()}». " +
                "Los estados avanzan en orden.");

        if (tramite.AliadoId == null)
            return ResultadoOperacion.Fallo("Asigne un aliado antes de continuar con la gestión.");

        if (nuevo == EstadoTramite.EnTransito)
            return ResultadoOperacion.Fallo("Para pasar a «En tránsito» registre el envío con su número de guía.");

        if (aliadoIdRestriccion.HasValue && nuevo >= EstadoTramite.EnTransito)
            return ResultadoOperacion.Fallo("Los estados de envío y entrega los registra Aliada Legal.");

        tramite.ActualizarEstado(nuevo, responsable, comentario);
        await _tramites.GuardarCambiosAsync();
        return ResultadoOperacion.Ok();
    }

    // ---------------------------------------------------------------- RF08
    public async Task<ResultadoOperacion> RegistrarEnvioAsync(int tramiteId, string numeroGuia,
        string empresaEnvio, DateTime fechaDespacho, string responsable)
    {
        var tramite = await _tramites.ObtenerPorIdAsync(tramiteId);
        if (tramite == null)
            return ResultadoOperacion.Fallo("El trámite no existe.");

        var listoParaEnviar = tramite.Estado == EstadoTramite.EnTransito || tramite.PuedeCambiarA(EstadoTramite.EnTransito);
        if (!listoParaEnviar)
            return ResultadoOperacion.Fallo("El envío se registra cuando el documento ya fue obtenido (y apostillado, si aplica).");

        var guia = numeroGuia.Trim();
        var empresa = empresaEnvio.Trim();
        var despacho = DateTime.SpecifyKind(fechaDespacho.Date, DateTimeKind.Utc);

        if (tramite.Envio == null)
        {
            tramite.Envio = new Envio
            {
                NumeroGuia = guia,
                EmpresaEnvio = empresa,
                FechaDespacho = despacho
            };
        }
        else
        {
            tramite.Envio.NumeroGuia = guia;
            tramite.Envio.EmpresaEnvio = empresa;
            tramite.Envio.FechaDespacho = despacho;
        }

        if (tramite.Estado != EstadoTramite.EnTransito)
            tramite.ActualizarEstado(EstadoTramite.EnTransito, responsable, $"Envío con {empresa}, guía {guia}.");

        await _tramites.GuardarCambiosAsync();
        return ResultadoOperacion.Ok();
    }

    // ------------------------------------------------------------- Consultas
    public Task<IReadOnlyList<Tramite>> ListarAsync(TramiteFiltro filtro, int? limite = null) =>
        _tramites.BuscarAsync(filtro, limite);

    public async Task<Tramite?> ObtenerAsync(int id, int? aliadoIdRestriccion)
    {
        var tramite = await _tramites.ObtenerPorIdAsync(id);
        if (tramite == null) return null;
        if (aliadoIdRestriccion.HasValue && tramite.AliadoId != aliadoIdRestriccion) return null;
        return tramite;
    }

    // ---------------------------------------------------------------- RF06
    public async Task<TramiteSeguimientoDto?> ConsultarSeguimientoAsync(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;

        var tramite = await _tramites.ObtenerPorCodigoAsync(codigo.Trim().ToUpperInvariant());
        if (tramite == null) return null;

        var requiereApostilla = tramite.TipoDocumento?.RequiereApostilla ?? false;
        var recorrido = EstadoTramiteFlujo.Recorrido(requiereApostilla);
        var fechasPorEstado = tramite.Historial
            .GroupBy(h => h.EstadoNuevo)
            .ToDictionary(g => g.Key, g => g.Min(h => h.Fecha));

        var dto = new TramiteSeguimientoDto
        {
            CodigoSeguimiento = tramite.CodigoSeguimiento,
            TipoDocumento = tramite.TipoDocumento?.Nombre ?? string.Empty,
            PaisDestino = tramite.PaisDestino?.Nombre ?? string.Empty,
            Estado = tramite.Estado.NombreMostrable(),
            EstadoClave = tramite.Estado.ClaveCss(),
            Progreso = EstadoTramiteFlujo.Progreso(tramite.Estado, requiereApostilla),
            FechaSolicitud = tramite.FechaSolicitud,
            UltimaActualizacion = tramite.FechaUltimaActualizacion,
            EmpresaEnvio = tramite.Envio?.EmpresaEnvio,
            NumeroGuia = tramite.Envio?.NumeroGuia
        };

        var indiceActual = recorrido.ToList().IndexOf(tramite.Estado);
        for (var i = 0; i < recorrido.Count; i++)
        {
            var estado = recorrido[i];
            dto.LineaDeTiempo.Add(new HitoSeguimientoDto
            {
                Estado = estado.NombreMostrable(),
                EstadoClave = estado.ClaveCss(),
                Fecha = fechasPorEstado.TryGetValue(estado, out var fecha) ? fecha : null,
                Completado = i <= indiceActual,
                EsActual = i == indiceActual
            });
        }

        return dto;
    }

    public async Task<ResumenTramitesDto> ObtenerResumenAsync(int? aliadoId)
    {
        var porEstado = await _tramites.ContarPorEstadoAsync(aliadoId);
        var total = porEstado.Values.Sum();
        var entregados = porEstado.TryGetValue(EstadoTramite.Entregado, out var n) ? n : 0;

        return new ResumenTramitesDto
        {
            Total = total,
            Entregados = entregados,
            Activos = total - entregados,
            PorEstado = porEstado
        };
    }

    // --------------------------------------------------------------- Privado
    private async Task<string> GenerarCodigoUnicoAsync()
    {
        for (var intento = 0; intento < 10; intento++)
        {
            var sufijo = new string(Enumerable.Range(0, 6)
                .Select(_ => AlfabetoCodigo[RandomNumberGenerator.GetInt32(AlfabetoCodigo.Length)])
                .ToArray());
            var codigo = $"AL-{DateTime.UtcNow.Year}-{sufijo}";

            if (!await _tramites.CodigoExisteAsync(codigo))
                return codigo;
        }

        throw new InvalidOperationException("No fue posible generar un código de seguimiento único.");
    }
}
