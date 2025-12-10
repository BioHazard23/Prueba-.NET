using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Services.Interfaces;

namespace TalentoPlus.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class ImportController : Controller
{
    private readonly IExcelService _excelService;

    public ImportController(IExcelService excelService)
    {
        _excelService = excelService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            TempData["Error"] = "Por favor seleccione un archivo Excel";
            return RedirectToAction(nameof(Index));
        }

        var extension = Path.GetExtension(archivo.FileName).ToLower();
        if (extension != ".xlsx" && extension != ".xls")
        {
            TempData["Error"] = "El archivo debe ser un Excel (.xlsx o .xls)";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            using var stream = archivo.OpenReadStream();
            var resultado = await _excelService.ImportarEmpleadosAsync(stream);

            if (resultado.Errores > 0)
            {
                TempData["Warning"] = string.Join("<br/>", resultado.Mensajes);
            }
            else
            {
                TempData["Success"] = resultado.Mensajes.FirstOrDefault() ?? "Importación completada";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al procesar el archivo: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}

