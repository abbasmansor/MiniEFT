using MiniEft.Domain.Entities;

namespace MiniEft.Application.Abstractions;

public interface ILedgerRepository
{
    Task AddRangeAsync(IEnumerable<LedgerEntry> entries, CancellationToken ct);

    Task<(IReadOnlyList<LedgerEntry> Items, int Total)> GetByAccountAsync(
        Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken ct);
}
