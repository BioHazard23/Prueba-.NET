using TalentoPlus.Application.DTOs;
using TalentoPlus.Application.Services.Interfaces;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Domain.Interfaces;

namespace TalentoPlus.Application.Services.Implementations;

public class EmpleadoService : IEmpleadoService
{
    private readonly IUnitOfWork _unitOfWork;

    public EmpleadoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<EmpleadoDto>> GetAllAsync()
    {
        var empleados = await _unitOfWork.Empleados.GetAllWithDetailsAsync();
        return empleados.Select(MapToDto);
    }

    public async Task<EmpleadoDto?> GetByIdAsync(int id)
    {
        var empleado = await _unitOfWork.Empleados.GetWithDetailsAsync(id);
        return empleado == null ? null : MapToDto(empleado);
    }

    public async Task<EmpleadoDto?> GetByDocumentoAsync(string documento)
    {
        var empleado = await _unitOfWork.Empleados.GetByDocumentoAsync(documento);
        return empleado == null ? null : MapToDto(empleado);
    }

    public async Task<EmpleadoDto?> GetByEmailAsync(string email)
    {
        var empleado = await _unitOfWork.Empleados.GetByEmailAsync(email);
        return empleado == null ? null : MapToDto(empleado);
    }

    public async Task<EmpleadoDto> CreateAsync(EmpleadoCreateDto dto)
    {
        var empleado = new Empleado
        {
            Documento = dto.Documento,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = DateTime.SpecifyKind(dto.FechaNacimiento, DateTimeKind.Utc),
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Salario = dto.Salario,
            FechaIngreso = DateTime.SpecifyKind(dto.FechaIngreso, DateTimeKind.Utc),
            Estado = dto.Estado,
            NivelEducativo = dto.NivelEducativo,
            PerfilProfesional = dto.PerfilProfesional,
            DepartamentoId = dto.DepartamentoId,
            CargoId = dto.CargoId
        };

        await _unitOfWork.Empleados.AddAsync(empleado);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(empleado.Id) ?? throw new Exception("Error al crear empleado");
    }

    public async Task<EmpleadoDto> UpdateAsync(EmpleadoUpdateDto dto)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Empleado con ID {dto.Id} no encontrado");

        empleado.Documento = dto.Documento;
        empleado.Nombres = dto.Nombres;
        empleado.Apellidos = dto.Apellidos;
        empleado.FechaNacimiento = DateTime.SpecifyKind(dto.FechaNacimiento, DateTimeKind.Utc);
        empleado.Direccion = dto.Direccion;
        empleado.Telefono = dto.Telefono;
        empleado.Email = dto.Email;
        empleado.Salario = dto.Salario;
        empleado.FechaIngreso = DateTime.SpecifyKind(dto.FechaIngreso, DateTimeKind.Utc);
        empleado.Estado = dto.Estado;
        empleado.NivelEducativo = dto.NivelEducativo;
        empleado.PerfilProfesional = dto.PerfilProfesional;
        empleado.DepartamentoId = dto.DepartamentoId;
        empleado.CargoId = dto.CargoId;

        await _unitOfWork.Empleados.UpdateAsync(empleado);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(empleado.Id) ?? throw new Exception("Error al actualizar empleado");
    }

    public async Task DeleteAsync(int id)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Empleado con ID {id} no encontrado");

        await _unitOfWork.Empleados.DeleteAsync(empleado);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ExistsByDocumentoAsync(string documento)
    {
        return await _unitOfWork.Empleados.ExistsAsync(e => e.Documento == documento);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _unitOfWork.Empleados.ExistsAsync(e => e.Email.ToLower() == email.ToLower());
    }

    public async Task<int> CountByEstadoAsync(EstadoEmpleado estado)
    {
        return await _unitOfWork.Empleados.CountByEstadoAsync(estado);
    }

    public async Task<int> CountTotalAsync()
    {
        return await _unitOfWork.Empleados.CountAsync();
    }

    public async Task<DashboardDto> GetDashboardStatsAsync()
    {
        var empleados = await _unitOfWork.Empleados.GetAllWithDetailsAsync();
        var empleadosList = empleados.ToList();

        return new DashboardDto
        {
            TotalEmpleados = empleadosList.Count,
            EmpleadosActivos = empleadosList.Count(e => e.Estado == EstadoEmpleado.Activo),
            EmpleadosInactivos = empleadosList.Count(e => e.Estado == EstadoEmpleado.Inactivo),
            EmpleadosVacaciones = empleadosList.Count(e => e.Estado == EstadoEmpleado.Vacaciones),
            EmpleadosPorDepartamento = empleadosList
                .GroupBy(e => e.Departamento.Nombre)
                .Select(g => new DepartamentoStatsDto
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(d => d.Cantidad)
                .ToList(),
            EmpleadosPorCargo = empleadosList
                .GroupBy(e => e.Cargo.Nombre)
                .Select(g => new CargoStatsDto
                {
                    Nombre = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(c => c.Cantidad)
                .ToList()
        };
    }

    private static EmpleadoDto MapToDto(Empleado empleado)
    {
        return new EmpleadoDto
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
            DepartamentoNombre = empleado.Departamento?.Nombre ?? string.Empty,
            CargoId = empleado.CargoId,
            CargoNombre = empleado.Cargo?.Nombre ?? string.Empty
        };
    }
}

