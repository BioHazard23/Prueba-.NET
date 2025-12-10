using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Domain.Entities;

public class Empleado : BaseEntity
{
    // Datos personales
    public string Documento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Datos laborales
    public decimal Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;
    public NivelEducativo NivelEducativo { get; set; }
    public string? PerfilProfesional { get; set; }
    
    // Relaciones
    public int DepartamentoId { get; set; }
    public virtual Departamento Departamento { get; set; } = null!;
    
    public int CargoId { get; set; }
    public virtual Cargo Cargo { get; set; } = null!;
    
    // Para autenticación en la API (opcional, se usará con Identity)
    public string? PasswordHash { get; set; }
    
    // Propiedades calculadas
    public string NombreCompleto => $"{Nombres} {Apellidos}";
    
    public int Edad
    {
        get
        {
            var hoy = DateTime.Today;
            var edad = hoy.Year - FechaNacimiento.Year;
            if (FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }
    }
    
    public int AntiguedadAnios
    {
        get
        {
            var hoy = DateTime.Today;
            var antiguedad = hoy.Year - FechaIngreso.Year;
            if (FechaIngreso.Date > hoy.AddYears(-antiguedad)) antiguedad--;
            return antiguedad;
        }
    }
}

