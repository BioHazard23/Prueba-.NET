using System.ComponentModel.DataAnnotations;
using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Application.DTOs;

public class EmpleadoDto
{
    public int Id { get; set; }
    public string Documento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NombreCompleto => $"{Nombres} {Apellidos}";
    public DateTime FechaNacimiento { get; set; }
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public EstadoEmpleado Estado { get; set; }
    public string EstadoNombre => Estado.ToString();
    public NivelEducativo NivelEducativo { get; set; }
    public string NivelEducativoNombre => NivelEducativo.ToString();
    public string? PerfilProfesional { get; set; }
    public int DepartamentoId { get; set; }
    public string DepartamentoNombre { get; set; } = string.Empty;
    public int CargoId { get; set; }
    public string CargoNombre { get; set; } = string.Empty;
}

public class EmpleadoCreateDto
{
    [Required(ErrorMessage = "El documento es requerido")]
    [StringLength(20, ErrorMessage = "El documento no puede exceder 20 caracteres")]
    public string Documento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los nombres son requeridos")]
    [StringLength(100, ErrorMessage = "Los nombres no pueden exceder 100 caracteres")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los apellidos son requeridos")]
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres")]
    public string Apellidos { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es requerida")]
    [DataType(DataType.Date)]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "La dirección es requerida")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es requerido")]
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El email no es válido")]
    [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "El salario es requerido")]
    [Range(0, double.MaxValue, ErrorMessage = "El salario debe ser mayor o igual a 0")]
    public decimal Salario { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es requerida")]
    [DataType(DataType.Date)]
    public DateTime FechaIngreso { get; set; }

    [Required(ErrorMessage = "El estado es requerido")]
    public EstadoEmpleado Estado { get; set; } = EstadoEmpleado.Activo;

    [Required(ErrorMessage = "El nivel educativo es requerido")]
    public NivelEducativo NivelEducativo { get; set; }

    [StringLength(500, ErrorMessage = "El perfil profesional no puede exceder 500 caracteres")]
    public string? PerfilProfesional { get; set; }

    [Required(ErrorMessage = "El departamento es requerido")]
    public int DepartamentoId { get; set; }

    [Required(ErrorMessage = "El cargo es requerido")]
    public int CargoId { get; set; }
}

public class EmpleadoUpdateDto : EmpleadoCreateDto
{
    public int Id { get; set; }
}

