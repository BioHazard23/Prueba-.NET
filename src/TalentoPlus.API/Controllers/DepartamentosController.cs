using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Services.Interfaces;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartamentosController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public DepartamentosController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    /// <summary>
    /// Obtiene la lista de todos los departamentos (público)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartamentoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var departamentos = await _catalogoService.GetDepartamentosAsync();
        return Ok(ApiResponse<IEnumerable<DepartamentoDto>>.Ok(departamentos, "Departamentos obtenidos correctamente"));
    }

    /// <summary>
    /// Obtiene un departamento por su ID (público)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DepartamentoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var departamento = await _catalogoService.GetDepartamentoByIdAsync(id);
        
        if (departamento == null)
        {
            return NotFound(ApiResponse.Fail("Departamento no encontrado"));
        }

        return Ok(ApiResponse<DepartamentoDto>.Ok(departamento));
    }
}

