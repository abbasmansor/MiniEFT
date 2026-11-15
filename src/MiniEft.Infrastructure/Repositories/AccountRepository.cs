using Microsoft.EntityFrameworkCore;
using MiniEft.Application.Abstractions;
using MiniEft.Domain.Entities;
using MiniEft.Infrastructure.Persistence;

namespace MiniEft.Infrastructure.Repositories;

public sealed class AccountRepository(AppDbContext db) : IAccountRepository
{
    public Task<Account?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
        db.Accounts.AnyAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyDictionary<Guid, Account>> GetForUpdateAsync(Guid a, Guid b, CancellationToken ct)
    {
       
        var ids = new[] { a, b };
        var rows = await db.Accounts
            .FromSqlInterpolated($"SELECT * FROM accounts WHERE id = ANY({ids}) ORDER BY id FOR NO KEY UPDATE")
            .AsTracking()
            .ToListAsync(ct);
        return rows.ToDictionary(x => x.Id);
    }
}
