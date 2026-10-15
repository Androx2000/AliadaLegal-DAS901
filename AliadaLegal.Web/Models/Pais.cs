using System.ComponentModel.DataAnnotations;

namespace AliadaLegal.Web.Models;

public class Pais
{
    
    [Key]
    public int Id {get; set; }

[Required]
[StringLength(100)]
public string Nombre {get; set; } = string.Empty;

[Required]
[StringLength(2, MinimumLength = 2)]
public string CodigoIso { get; set; } = string.Empty;

}