namespace TalentoPlus.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEmpleadoRepository Empleados { get; }
    IDepartamentoRepository Departamentos { get; }
    ICargoRepository Cargos { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

