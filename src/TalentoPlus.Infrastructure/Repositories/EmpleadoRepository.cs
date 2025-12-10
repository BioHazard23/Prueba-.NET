using Microsoft.EntityFrameworkCore;
using TalentoPlus.Domain.Entities;
using TalentoPlus.Domain.Enums;
using TalentoPlus.Domain.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

public class EmpleadoRepository : Repository<Empleado>, IEmpleadoRepository
{
    public EmpleadoRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Empleado?> GetByDocumentoAsync(string documento)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .FirstOrDefaultAsync(e => e.Documento == documento);
    }

    public async Task<Empleado?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
    }

    public async Task<Empleado?> GetByDocumentoAndEmailAsync(string documento, string email)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .FirstOrDefaultAsync(e => e.Documento == documento && e.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(int departamentoId)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .Where(e => e.DepartamentoId == departamentoId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Empleado>> GetByEstadoAsync(EstadoEmpleado estado)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .Where(e => e.Estado == estado)
            .ToListAsync();
    }

    public async Task<IEnumerable<Empleado>> GetByCargoAsync(int cargoId)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .Where(e => e.CargoId == cargoId)
            .ToListAsync();
    }

    public async Task<Empleado?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Empleado>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .OrderBy(e => e.Apellidos)
            .ThenBy(e => e.Nombres)
            .ToListAsync();
    }

    public async Task<int> CountByEstadoAsync(EstadoEmpleado estado)
    {
        return await _dbSet.CountAsync(e => e.Estado == estado);
    }

    public async Task<int> CountByDepartamentoAsync(int departamentoId)
    {
        return await _dbSet.CountAsync(e => e.DepartamentoId == departamentoId);
    }

    public async Task<int> CountByCargoAsync(string cargoNombre)
    {
        return await _dbSet
            .Include(e => e.Cargo)
            .CountAsync(e => e.Cargo.Nombre.ToLower() == cargoNombre.ToLower());
    }
}

