using AliadaLegal.Web.Dtos;
using AliadaLegal.Web.Helpers;
using AliadaLegal.Web.Models;
using AliadaLegal.Web.Repositories;

namespace AliadaLegal.Web.Services;

public class SeguimientoService : ISeguimientoService
{
    private readonly ITramiteRepository _tramites;

    public SeguimientoService(ITramiteRepository tramites)
    {
        _tramites = tramites;
    }

    public async Task<TramiteSeguimientoDto?> ConsultarAsync(string codigo)
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
}
