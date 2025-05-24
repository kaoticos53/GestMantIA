using GestMantIA.Core.Entities;
using GestMantIA.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GestMantIA.Infrastructure.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación.
/// Configura las entidades y sus relaciones con Entity Framework Core.
/// </summary>
public class GestMantIADbContext : DbContext
{
    public GestMantIADbContext(DbContextOptions<GestMantIADbContext> options)
        : base(options)
    {
    }

    // DbSets para cada entidad
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<UsuarioRol> UsuarioRoles => Set<UsuarioRol>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones de entidades
        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new RolConfiguration());
        modelBuilder.ApplyConfiguration(new PermisoConfiguration());
        modelBuilder.ApplyConfiguration(new UsuarioRolConfiguration());
        modelBuilder.ApplyConfiguration(new RolPermisoConfiguration());

        // Configurar relaciones y restricciones
        ConfigurarRelaciones(modelBuilder);
        
        // Configurar índices para mejorar el rendimiento
        ConfigurarIndices(modelBuilder);
        
        // Configurar datos iniciales
        ConfigurarDatosIniciales(modelBuilder);
    }

    private static void ConfigurarRelaciones(ModelBuilder modelBuilder)
    {
        // Configuración de la relación muchos a muchos Usuario-Rol
        modelBuilder.Entity<UsuarioRol>()
            .HasKey(ur => new { ur.UsuarioId, ur.RolId });

        modelBuilder.Entity<UsuarioRol>()
            .HasOne(ur => ur.Usuario)
            .WithMany(u => u.UsuarioRoles)
            .HasForeignKey(ur => ur.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UsuarioRol>()
            .HasOne(ur => ur.Rol)
            .WithMany(r => r.UsuarioRoles)
            .HasForeignKey(ur => ur.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configuración de la relación muchos a muchos Rol-Permiso
        modelBuilder.Entity<RolPermiso>()
            .HasKey(rp => new { rp.RolId, rp.PermisoId });

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Rol)
            .WithMany(r => r.RolPermisos)
            .HasForeignKey(rp => rp.RolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RolPermiso>()
            .HasOne(rp => rp.Permiso)
            .WithMany(p => p.RolPermisos)
            .HasForeignKey(rp => rp.PermisoId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigurarIndices(ModelBuilder modelBuilder)
    {
        // Índices para campos únicos
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();
            
        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Nombre)
            .IsUnique();
            
        modelBuilder.Entity<Permiso>()
            .HasIndex(p => p.Nombre)
            .IsUnique();
    }

    private static void ConfigurarDatosIniciales(ModelBuilder modelBuilder)
    {
        // Aquí se agregarán datos iniciales en la siguiente iteración
    }
}
