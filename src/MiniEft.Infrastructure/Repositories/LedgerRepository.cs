using Microsoft.EntityFrameworkCore;
using MiniEft.Application.Abstractions;
using MiniEft.Domain.Entities;
using MiniEft.Infrastructure.Persistence;

namespace MiniEft.Infrastructure.Repositories;

public sealed class LedgerRepository(AppDbContext db) : ILedgerRepository
{
    public Task AddRangeAsync(IEnumerable<LedgerEntry> entries, CancellationToken ct)
    {
        db.LedgerEntries.AddRange(entries);
        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<LedgerEntry> Items, int Total)> GetByAccountAsync(
        Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken ct)
    {
        var query = db.LedgerEntries.AsNoTracking().Where(e => e.AccountId == accountId);
        if (from is not null) query = query.Where(e => e.CreatedAt >= from);
        if (to is not null) query = query.Where(e => e.CreatedAt <= to);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(e => e.CreatedAt).ThenByDescending(e => e.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }
}
