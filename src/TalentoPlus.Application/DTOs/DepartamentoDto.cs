namespace TalentoPlus.Application.DTOs;

public class DepartamentoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int CantidadEmpleados { get; set; }
}

