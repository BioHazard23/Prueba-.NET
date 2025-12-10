using Microsoft.EntityFrameworkCore;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

public class DepartamentoRepository : Repository<Departamento>, IDepartamentoRepository
{
    public DepartamentoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Departamento?> GetByNombreAsync(string nombre)
    {
        return await _dbSet.FirstOrDefaultAsync(d => d.Nombre.ToLower() == nombre.ToLower());
    }

    public async Task<Departamento?> GetWithEmpleadosAsync(int id)
    {
        return await _dbSet
            .Include(d => d.Empleados)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<IEnumerable<(Departamento Departamento, int CantidadEmpleados)>> GetAllWithEmpleadosCountAsync()
    {
        var result = await _dbSet
            .Select(d => new 
            { 
                Departamento = d, 
                CantidadEmpleados = d.Empleados.Count 
            })
            .ToListAsync();

        return result.Select(r => (r.Departamento, r.CantidadEmpleados));
    }

    public async Task<(Departamento Departamento, int CantidadEmpleados)?> GetByIdWithEmpleadosCountAsync(int id)
    {
        var result = await _dbSet
            .Where(d => d.Id == id)
            .Select(d => new 
            { 
                Departamento = d, 
                CantidadEmpleados = d.Empleados.Count 
            })
            .FirstOrDefaultAsync();

        if (result == null) return null;
        
        return (result.Departamento, result.CantidadEmpleados);
    }
}

