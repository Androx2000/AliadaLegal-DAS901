using AliadaLegal.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AliadaLegal.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<TipoDocumento> TiposDocumento => Set<TipoDocumento>();
    public DbSet<Tramite> Tramites => Set<Tramite>();
    public DbSet<Aliado> Aliados => Set<Aliado>();
    public DbSet<Envio> Envios => Set<Envio>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Un rol puede tener muchos usuarios.
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un usuario puede estar vinculado a un aliado.
        modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Aliado)
            .WithMany()
            .HasForeignKey(u => u.AliadoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cada correo de usuario debe ser único.
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Correo)
            .IsUnique();

        // Cada nombre de rol debe ser único.
        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.NombreRol)
            .IsUnique();

        // Un cliente puede tener varios trámites.
        modelBuilder.Entity<Tramite>()
            .HasOne(t => t.Cliente)
            .WithMany(c => c.Tramites)
            .HasForeignKey(t => t.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un país puede tener varios trámites.
        modelBuilder.Entity<Tramite>()
            .HasOne(t => t.Pais)
            .WithMany(p => p.Tramites)
            .HasForeignKey(t => t.PaisId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un documento puede aparecer en varios trámites.
        modelBuilder.Entity<Tramite>()
            .HasOne(t => t.TipoDocumento)
            .WithMany(d => d.Tramites)
            .HasForeignKey(t => t.TipoDocumentoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Un aliado puede gestionar varios trámites.
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

        // Un trámite puede tener un envío.
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