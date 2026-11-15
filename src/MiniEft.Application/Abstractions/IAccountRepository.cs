using MiniEft.Domain.Entities;

namespace MiniEft.Application.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyDictionary<Guid, Account>> GetForUpdateAsync(Guid a, Guid b, CancellationToken ct);
}
