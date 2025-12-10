using Microsoft.EntityFrameworkCore.Storage;
using TalentoPlus.Domain.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    private IEmpleadoRepository? _empleados;
    private IDepartamentoRepository? _departamentos;
    private ICargoRepository? _cargos;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IEmpleadoRepository Empleados => 
        _empleados ??= new EmpleadoRepository(_context);

    public IDepartamentoRepository Departamentos => 
        _departamentos ??= new DepartamentoRepository(_context);

    public ICargoRepository Cargos => 
        _cargos ??= new CargoRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}

