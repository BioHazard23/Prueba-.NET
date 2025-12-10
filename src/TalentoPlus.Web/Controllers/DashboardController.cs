using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Services.Interfaces;

namespace TalentoPlus.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class DashboardController : Controller
{
    private readonly IEmpleadoService _empleadoService;
    private readonly IAIService _aiService;

    public DashboardController(IEmpleadoService empleadoService, IAIService aiService)
    {
        _empleadoService = empleadoService;
        _aiService = aiService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _empleadoService.GetDashboardStatsAsync();
        return View(stats);
    }

    [HttpPost]
    public async Task<IActionResult> AskAI([FromBody] AIQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Query))
        {
            return Json(new { success = false, error = "La pregunta no puede estar vacía." });
        }

        var result = await _aiService.ProcessQueryAsync(request.Query);
        
        return Json(new 
        { 
            success = result.Success, 
            response = result.Response,
            error = result.Error
        });
    }
}

public class AIQueryRequest
{
    public string Query { get; set; } = string.Empty;
}
