using TalentoPlus.Application.DTOs;

namespace TalentoPlus.Application.Services.Interfaces;

public interface IExcelService
{
    Task<ExcelImportResultDto> ImportarEmpleadosAsync(Stream excelStream);
}

public class ExcelImportResultDto
{
    public int TotalFilas { get; set; }
    public int Insertados { get; set; }
    public int Actualizados { get; set; }
    public int Errores { get; set; }
    public List<string> Mensajes { get; set; } = new();
}

