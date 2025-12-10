using TalentoPlus.Application.DTOs;
using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Application.Services.Interfaces;

public interface IEmpleadoService
{
    Task<IEnumerable<EmpleadoDto>> GetAllAsync();
    Task<EmpleadoDto?> GetByIdAsync(int id);
    Task<EmpleadoDto?> GetByDocumentoAsync(string documento);
    Task<EmpleadoDto?> GetByEmailAsync(string email);
    Task<EmpleadoDto> CreateAsync(EmpleadoCreateDto dto);
    Task<EmpleadoDto> UpdateAsync(EmpleadoUpdateDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsByDocumentoAsync(string documento);
    Task<bool> ExistsByEmailAsync(string email);
    Task<int> CountByEstadoAsync(EstadoEmpleado estado);
    Task<int> CountTotalAsync();
    Task<DashboardDto> GetDashboardStatsAsync();
}

