using Microsoft.EntityFrameworkCore;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

public class CargoRepository : Repository<Cargo>, ICargoRepository
{
    public CargoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Cargo?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<Cargo?> GetWithEmpleadosAsync(int id)
    {
        return await _dbSet
            .Include(c => c.Empleados)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}

