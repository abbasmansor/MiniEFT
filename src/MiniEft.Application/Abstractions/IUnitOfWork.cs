namespace MiniEft.Application.Abstractions;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken ct);

    Task BeginTransactionAsync(CancellationToken ct);
    Task CommitAsync(CancellationToken ct);

    Task RollbackAsync(CancellationToken ct);
}
