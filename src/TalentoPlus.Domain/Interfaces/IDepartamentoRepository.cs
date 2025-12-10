using TalentoPlus.Domain.Entities;

namespace TalentoPlus.Domain.Interfaces;

public interface IDepartamentoRepository : IRepository<Departamento>
{
    Task<Departamento?> GetByNombreAsync(string nombre);
    Task<Departamento?> GetWithEmpleadosAsync(int id);
    Task<IEnumerable<(Departamento Departamento, int CantidadEmpleados)>> GetAllWithEmpleadosCountAsync();
    Task<(Departamento Departamento, int CantidadEmpleados)?> GetByIdWithEmpleadosCountAsync(int id);
}

