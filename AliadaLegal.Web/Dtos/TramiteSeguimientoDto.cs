namespace AliadaLegal.Web.Dtos;

public class TramiteSeguimientoDto

{
    
    public string CodigoSeguimiento {get; set; }
        =string.Empty;

    public string TipoDocumento {get; set; }
        = string.Empty;

    public string Pais {get; set; }
       = string.Empty;

    public string Estado { get; set; }
      = string.Empty;

    public DateTime FechaSolicitud { get; set; }

}