using AliadaLegal.Web.Models;

namespace AliadaLegal.Web.Dtos;

public class ResumenTramitesDto
{
    public int Total { get; set; }
    public int Activos { get; set; }
    public int Entregados { get; set; }
    public Dictionary<EstadoTramite, int> PorEstado { get; set; } = new();
}
