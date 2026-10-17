using System.Collections.Generic;

namespace AliadaLegal.Web.Dtos;

public class TramiteSeguimientoDto
{
    public string CodigoSeguimiento { get; set; }
        = string.Empty;

    public string TipoDocumento { get; set; }
        = string.Empty;

    public string PaisDestino { get; set; }
        = string.Empty;

    public string Estado { get; set; }
        = string.Empty;

    public string EstadoClave { get; set; }
        = string.Empty;

    public int Progreso { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public DateTime UltimaActualizacion { get; set; }

    public string? EmpresaEnvio { get; set; }

    public string? NumeroGuia { get; set; }

    public List<HitoSeguimientoDto> LineaDeTiempo { get; set; }
        = new();
}