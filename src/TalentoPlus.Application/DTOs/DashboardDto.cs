namespace TalentoPlus.Application.DTOs;

public class DashboardDto
{
    public int TotalEmpleados { get; set; }
    public int EmpleadosActivos { get; set; }
    public int EmpleadosInactivos { get; set; }
    public int EmpleadosVacaciones { get; set; }
    public List<DepartamentoStatsDto> EmpleadosPorDepartamento { get; set; } = new();
    public List<CargoStatsDto> EmpleadosPorCargo { get; set; } = new();
}

public class DepartamentoStatsDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

public class CargoStatsDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
}

