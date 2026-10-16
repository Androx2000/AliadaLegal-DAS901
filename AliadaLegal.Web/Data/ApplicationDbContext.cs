using AliadaLegal.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes { get; set; }
        = null!;

    public DbSet<Pais> Paises { get; set; }
        = null!;

    public DbSet<TipoDocumento> TiposDocumento { get; set; }
        = null!;
    public DbSet<Tramite> Tramites { get; set; }
        = null!;

    public DbSet<Aliado> Aliados { get; set; }
        = null!;
    public DbSet<Envio> Envios { get; set; }
        = null!;

}