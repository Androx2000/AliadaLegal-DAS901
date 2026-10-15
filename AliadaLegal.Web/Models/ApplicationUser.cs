using Microsoft.AspNetCore.Identity;

namespace AliadaLegal.Web.Models;

public class ApplicactionUser : IdentityUser
{
    public string NombreCompleto {get; set;}
    = string.Empty;
}