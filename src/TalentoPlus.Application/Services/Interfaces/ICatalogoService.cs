using TalentoPlus.Application.DTOs;

namespace TalentoPlus.Application.Services.Interfaces;

public interface ICatalogoService
{
    Task<IEnumerable<DepartamentoDto>> GetDepartamentosAsync();
    Task<IEnumerable<CargoDto>> GetCargosAsync();
    Task<DepartamentoDto?> GetDepartamentoByIdAsync(int id);
    Task<CargoDto?> GetCargoByIdAsync(int id);
}

