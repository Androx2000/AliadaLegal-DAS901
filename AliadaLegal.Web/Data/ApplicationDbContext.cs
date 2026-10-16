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

    protected override void OnModelCreating(
    ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Un cliente puede tener muchos trámites.
    modelBuilder.Entity<Tramite>()
        .HasOne(t => t.Cliente)
        .WithMany(c => c.Tramites)
        .HasForeignKey(t => t.ClienteId)
        .OnDelete(DeleteBehavior.Restrict);

    // Un país puede tener muchos trámites.
    modelBuilder.Entity<Tramite>()
        .HasOne(t => t.Pais)
        .WithMany(p => p.Tramites)
        .HasForeignKey(t => t.PaisId)
        .OnDelete(DeleteBehavior.Restrict);

    // Un tipo de documento puede tener muchos trámites.
    modelBuilder.Entity<Tramite>()
        .HasOne(t => t.TipoDocumento)
        .WithMany(d => d.Tramites)
        .HasForeignKey(t => t.TipoDocumentoId)
        .OnDelete(DeleteBehavior.Restrict);

    // Un aliado puede tener muchos trámites.
    modelBuilder.Entity<Tramite>()
        .HasOne(t => t.Aliado)
        .WithMany(a => a.Tramites)
        .HasForeignKey(t => t.AliadoId)
        .OnDelete(DeleteBehavior.SetNull);

    // Cada aliado pertenece a un país.
    modelBuilder.Entity<Aliado>()
        .HasOne(a => a.Pais)
        .WithMany()
        .HasForeignKey(a => a.PaisId)
        .OnDelete(DeleteBehavior.Restrict);

    // Cada trámite puede tener un único envío.
    modelBuilder.Entity<Envio>()
        .HasOne(e => e.Tramite)
        .WithOne(t => t.Envio)
        .HasForeignKey<Envio>(e => e.TramiteId)
        .OnDelete(DeleteBehavior.Restrict);

    // Evitar códigos ISO duplicados.
    modelBuilder.Entity<Pais>()
        .HasIndex(p => p.CodigoIso)
        .IsUnique();
}

}