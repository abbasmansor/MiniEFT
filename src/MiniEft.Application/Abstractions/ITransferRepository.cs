using MiniEft.Domain.Entities;

namespace MiniEft.Application.Abstractions;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<Transfer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct);

    Task<Transfer?> GetByIdempotencyKeyAsync(string key, CancellationToken ct);

    Task AddAsync(Transfer transfer, CancellationToken ct);
}
