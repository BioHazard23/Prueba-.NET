using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalentoPlus.Domain.Entities;

namespace TalentoPlus.Infrastructure.Data.Configurations;

public class EmpleadoConfiguration : IEntityTypeConfiguration<Empleado>
{
    public void Configure(EntityTypeBuilder<Empleado> builder)
    {
        builder.ToTable("Empleados");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Documento)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(e => e.Documento)
            .IsUnique();

        builder.Property(e => e.Nombres)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Apellidos)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Telefono)
            .HasMaxLength(20);

        builder.Property(e => e.Direccion)
            .HasMaxLength(200);

        builder.Property(e => e.Salario)
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.PerfilProfesional)
            .HasMaxLength(500);

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(256);

        builder.Property(e => e.Estado)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.NivelEducativo)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relaciones
        builder.HasOne(e => e.Departamento)
            .WithMany(d => d.Empleados)
            .HasForeignKey(e => e.DepartamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Cargo)
            .WithMany(c => c.Empleados)
            .HasForeignKey(e => e.CargoId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignorar propiedades calculadas
        builder.Ignore(e => e.NombreCompleto);
        builder.Ignore(e => e.Edad);
        builder.Ignore(e => e.AntiguedadAnios);
    }
}

