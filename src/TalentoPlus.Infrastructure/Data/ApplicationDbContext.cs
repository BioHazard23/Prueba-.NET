using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Infrastructure.Data.Configurations;

namespace TalentoPlus.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public int? EmpleadoId { get; set; }
    public virtual Empleado? Empleado { get; set; }
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Departamento> Departamentos { get; set; }
    public DbSet<Cargo> Cargos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configuraciones
        modelBuilder.ApplyConfiguration(new EmpleadoConfiguration());
        modelBuilder.ApplyConfiguration(new DepartamentoConfiguration());
        modelBuilder.ApplyConfiguration(new CargoConfiguration());

        // Configurar relación ApplicationUser - Empleado
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Empleado)
            .WithMany()
            .HasForeignKey(u => u.EmpleadoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Seed de roles
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole 
            { 
                Id = "1", 
                Name = "Administrador", 
                NormalizedName = "ADMINISTRADOR" 
            },
            new IdentityRole 
            { 
                Id = "2", 
                Name = "Empleado", 
                NormalizedName = "EMPLEADO" 
            }
        );

        // Seed de departamentos
        modelBuilder.Entity<Departamento>().HasData(
            new Departamento { Id = 1, Nombre = "Contabilidad" },
            new Departamento { Id = 2, Nombre = "Logística" },
            new Departamento { Id = 3, Nombre = "Marketing" },
            new Departamento { Id = 4, Nombre = "Operaciones" },
            new Departamento { Id = 5, Nombre = "Recursos Humanos" },
            new Departamento { Id = 6, Nombre = "Tecnología" },
            new Departamento { Id = 7, Nombre = "Ventas" }
        );

        // Seed de cargos
        modelBuilder.Entity<Cargo>().HasData(
            new Cargo { Id = 1, Nombre = "Administrador" },
            new Cargo { Id = 2, Nombre = "Analista" },
            new Cargo { Id = 3, Nombre = "Auxiliar" },
            new Cargo { Id = 4, Nombre = "Coordinador" },
            new Cargo { Id = 5, Nombre = "Desarrollador" },
            new Cargo { Id = 6, Nombre = "Ingeniero" },
            new Cargo { Id = 7, Nombre = "Soporte Técnico" }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.FechaCreacion = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.FechaActualizacion = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}

