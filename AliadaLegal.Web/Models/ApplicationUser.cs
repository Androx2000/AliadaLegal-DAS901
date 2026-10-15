using Microsoft.AspNetCore.Identity;

namespace AliadaLegal.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string NombreCompleto { get; set; }
        = string.Empty;
}