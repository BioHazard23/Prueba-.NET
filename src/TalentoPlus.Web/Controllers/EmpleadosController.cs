using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Domain.Interfaces;

namespace TalentoPlus.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class EmpleadosController : Controller
{
    private readonly IEmpleadoService _empleadoService;
    private readonly ICatalogoService _catalogoService;
    private readonly IPdfService _pdfService;
    private readonly IUnitOfWork _unitOfWork;

    public EmpleadosController(
        IEmpleadoService empleadoService, 
        ICatalogoService catalogoService,
        IPdfService pdfService,
        IUnitOfWork unitOfWork)
    {
        _empleadoService = empleadoService;
        _catalogoService = catalogoService;
        _pdfService = pdfService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var empleados = await _empleadoService.GetAllAsync();
        return View(empleados);
    }

    public async Task<IActionResult> Details(int id)
    {
        var empleado = await _empleadoService.GetByIdAsync(id);
        if (empleado == null)
        {
            return NotFound();
        }
        return View(empleado);
    }

    public async Task<IActionResult> Create()
    {
        await CargarSelectLists();
        return View(new EmpleadoCreateDto
        {
            FechaNacimiento = DateTime.Today.AddYears(-25),
            FechaIngreso = DateTime.Today,
            Estado = EstadoEmpleado.Activo
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmpleadoCreateDto model)
    {
        if (!ModelState.IsValid)
        {
            await CargarSelectLists();
            return View(model);
        }

        // Validar documento único
        if (await _empleadoService.ExistsByDocumentoAsync(model.Documento))
        {
            ModelState.AddModelError("Documento", "Ya existe un empleado con este documento");
            await CargarSelectLists();
            return View(model);
        }

        // Validar email único
        if (await _empleadoService.ExistsByEmailAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Ya existe un empleado con este email");
            await CargarSelectLists();
            return View(model);
        }

        try
        {
            await _empleadoService.CreateAsync(model);
            TempData["Success"] = "Empleado creado exitosamente";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al crear empleado: {ex.Message}");
            await CargarSelectLists();
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var empleado = await _empleadoService.GetByIdAsync(id);
        if (empleado == null)
        {
            return NotFound();
        }

        var model = new EmpleadoUpdateDto
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
            Estado = empleado.Estado,
            NivelEducativo = empleado.NivelEducativo,
            PerfilProfesional = empleado.PerfilProfesional,
            DepartamentoId = empleado.DepartamentoId,
            CargoId = empleado.CargoId
        };

        await CargarSelectLists();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EmpleadoUpdateDto model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await CargarSelectLists();
            return View(model);
        }

        // Validar documento único (excluyendo el actual)
        var existingByDoc = await _empleadoService.GetByDocumentoAsync(model.Documento);
        if (existingByDoc != null && existingByDoc.Id != model.Id)
        {
            ModelState.AddModelError("Documento", "Ya existe otro empleado con este documento");
            await CargarSelectLists();
            return View(model);
        }

        // Validar email único (excluyendo el actual)
        var existingByEmail = await _empleadoService.GetByEmailAsync(model.Email);
        if (existingByEmail != null && existingByEmail.Id != model.Id)
        {
            ModelState.AddModelError("Email", "Ya existe otro empleado con este email");
            await CargarSelectLists();
            return View(model);
        }

        try
        {
            await _empleadoService.UpdateAsync(model);
            TempData["Success"] = "Empleado actualizado exitosamente";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Error al actualizar empleado: {ex.Message}");
            await CargarSelectLists();
            return View(model);
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var empleado = await _empleadoService.GetByIdAsync(id);
        if (empleado == null)
        {
            return NotFound();
        }
        return View(empleado);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _empleadoService.DeleteAsync(id);
            TempData["Success"] = "Empleado eliminado exitosamente";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al eliminar empleado: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> GenerarPdf(int id)
    {
        var empleado = await _unitOfWork.Empleados.GetWithDetailsAsync(id);
        if (empleado == null)
        {
            return NotFound();
        }

        try
        {
            var pdfBytes = _pdfService.GenerarHojaVida(empleado);
            var fileName = $"HojaVida_{empleado.Documento}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al generar PDF: {ex.Message}";
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task CargarSelectLists()
    {
        var departamentos = await _catalogoService.GetDepartamentosAsync();
        var cargos = await _catalogoService.GetCargosAsync();

        ViewBag.Departamentos = new SelectList(departamentos, "Id", "Nombre");
        ViewBag.Cargos = new SelectList(cargos, "Id", "Nombre");
        ViewBag.Estados = new SelectList(
            Enum.GetValues<EstadoEmpleado>().Select(e => new { Id = (int)e, Nombre = e.ToString() }),
            "Id", "Nombre");
        ViewBag.NivelesEducativos = new SelectList(
            Enum.GetValues<NivelEducativo>().Select(n => new { Id = (int)n, Nombre = n.ToString() }),
            "Id", "Nombre");
    }
}

