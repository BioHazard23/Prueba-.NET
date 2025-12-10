using TalentoPlus.Domain.Entities;

namespace TalentoPlus.Domain.Interfaces;

public interface ICargoRepository : IRepository<Cargo>
{
    Task<Cargo?> GetByNombreAsync(string nombre);
    Task<Cargo?> GetWithEmpleadosAsync(int id);
}

