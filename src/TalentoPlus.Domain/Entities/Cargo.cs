namespace TalentoPlus.Domain.Entities;

public class Cargo : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    
    // Navegación
    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}

