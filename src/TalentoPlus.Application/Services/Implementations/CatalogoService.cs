using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Interfaces;

namespace TalentoPlus.Application.Services.Implementations;

public class CatalogoService : ICatalogoService
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<DepartamentoDto>> GetDepartamentosAsync()
    {
        var departamentos = await _unitOfWork.Departamentos.GetAllWithEmpleadosCountAsync();
        return departamentos.Select(d => new DepartamentoDto
        {
            Id = d.Departamento.Id,
            Nombre = d.Departamento.Nombre,
            Descripcion = d.Departamento.Descripcion,
            CantidadEmpleados = d.CantidadEmpleados
        }).OrderBy(d => d.Nombre);
    }

    public async Task<IEnumerable<CargoDto>> GetCargosAsync()
    {
        var cargos = await _unitOfWork.Cargos.GetAllAsync();
        return cargos.Select(c => new CargoDto
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion
        }).OrderBy(c => c.Nombre);
    }

    public async Task<DepartamentoDto?> GetDepartamentoByIdAsync(int id)
    {
        var result = await _unitOfWork.Departamentos.GetByIdWithEmpleadosCountAsync(id);
        if (result == null) return null;

        var (departamento, cantidadEmpleados) = result.Value;
        return new DepartamentoDto
        {
            Id = departamento.Id,
            Nombre = departamento.Nombre,
            Descripcion = departamento.Descripcion,
            CantidadEmpleados = cantidadEmpleados
        };
    }

    public async Task<CargoDto?> GetCargoByIdAsync(int id)
    {
        var cargo = await _unitOfWork.Cargos.GetByIdAsync(id);
        if (cargo == null) return null;

        return new CargoDto
        {
            Id = cargo.Id,
            Nombre = cargo.Nombre,
            Descripcion = cargo.Descripcion
        };
    }
}

