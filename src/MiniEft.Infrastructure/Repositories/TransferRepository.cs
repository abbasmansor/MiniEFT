using Microsoft.EntityFrameworkCore;
using MiniEft.Application.Abstractions;
using MiniEft.Domain.Entities;
using MiniEft.Infrastructure.Persistence;

namespace MiniEft.Infrastructure.Repositories;

public sealed class TransferRepository(AppDbContext db) : ITransferRepository
{
    public Task<Transfer?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Transfers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Transfer?> GetByIdForUpdateAsync(Guid id, CancellationToken ct) =>
        (await db.Transfers
            .FromSqlInterpolated($"SELECT * FROM transfers WHERE id = {id} FOR NO KEY UPDATE")
            .AsTracking()
            .ToListAsync(ct)).SingleOrDefault();

    public Task<Transfer?> GetByIdempotencyKeyAsync(string key, CancellationToken ct) =>
        db.Transfers.AsNoTracking().FirstOrDefaultAsync(t => t.IdempotencyKey == key, ct);

    public async Task AddAsync(Transfer transfer, CancellationToken ct) =>
        await db.Transfers.AddAsync(transfer, ct);
}
