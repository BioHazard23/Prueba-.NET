using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;

namespace TalentoPlus.Domain.Interfaces;

public interface IEmpleadoRepository : IRepository<Empleado>
{
    Task<Empleado?> GetByDocumentoAsync(string documento);
    Task<Empleado?> GetByEmailAsync(string email);
    Task<Empleado?> GetByDocumentoAndEmailAsync(string documento, string email);
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId);
    Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado);
    Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId);
    Task<Empleado?> GetWithDetailsAsync(int id);
    Task<IEnumerable<Empleado>> GetAllWithDetailsAsync();
    Task<int> CountByEstadoAsync(EstadoEmpleado estado);
    Task<int> CountByDepartamentoAsync(int departamentoId);
    Task<int> CountByCargoAsync(string cargoNombre);
}

