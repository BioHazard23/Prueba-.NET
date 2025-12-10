using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Interfaces;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfService _pdfService;

    public EmpleadosController(IUnitOfWork unitOfWork, IPdfService pdfService)
    {
        _unitOfWork = unitOfWork;
        _pdfService = pdfService;
    }

    /// <summary>
    /// Obtiene la información del empleado autenticado (protegido con JWT)
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<EmpleadoApiDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyInfo()
    {
        var documento = User.FindFirst("documento")?.Value;
        
        if (string.IsNullOrEmpty(documento))
        {
            return Unauthorized(ApiResponse.Fail("Token inválido"));
        }

        var empleado = await _unitOfWork.Empleados.GetByDocumentoAsync(documento);
        
        if (empleado == null)
        {
            return NotFound(ApiResponse.Fail("Empleado no encontrado"));
        }

        var response = new EmpleadoApiDto
        {
            Id = empleado.Id,
            Documento = empleado.Documento,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            FechaNacimiento = empleado.FechaNacimiento,
            Direccion = empleado.Direccion,
            Telefono = empleado.Telefono,
            Email = empleado.Email,
            Salario = empleado.Salario,
            FechaIngreso = empleado.FechaIngreso,
            Estado = empleado.Estado.ToString(),
            NivelEducativo = empleado.NivelEducativo.ToString(),
            PerfilProfesional = empleado.PerfilProfesional,
            Departamento = empleado.Departamento?.Nombre ?? string.Empty,
            Cargo = empleado.Cargo?.Nombre ?? string.Empty
        };

        return Ok(ApiResponse<EmpleadoApiDto>.Ok(response, "Información obtenida correctamente"));
    }

    /// <summary>
    /// Descarga la hoja de vida del empleado autenticado en PDF (protegido con JWT)
    /// </summary>
    [HttpGet("me/hoja-vida")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadHojaVida()
    {
        var documento = User.FindFirst("documento")?.Value;
        
        if (string.IsNullOrEmpty(documento))
        {
            return Unauthorized(ApiResponse.Fail("Token inválido"));
        }

        var empleado = await _unitOfWork.Empleados.GetByDocumentoAsync(documento);
        
        if (empleado == null)
        {
            return NotFound(ApiResponse.Fail("Empleado no encontrado"));
        }

        try
        {
            var pdfBytes = _pdfService.GenerarHojaVida(empleado);
            var fileName = $"HojaVida_{empleado.Documento}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse.Fail($"Error al generar PDF: {ex.Message}"));
        }
    }
}
