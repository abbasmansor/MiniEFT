using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MiniEft.Application.Abstractions;
using MiniEft.Application.Exceptions;
using Npgsql;

namespace MiniEft.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private const string UniqueViolation = "23505";
    private IDbContextTransaction? _tx;

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: UniqueViolation })
        {
            db.ChangeTracker.Clear();
            throw new DuplicateKeyException("Unique constraint violated.");
        }
    }

    public async Task BeginTransactionAsync(CancellationToken ct) =>
        _tx = await db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_tx is null) return;
        await _tx.CommitAsync(ct);
        await _tx.DisposeAsync();
        _tx = null;
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_tx is not null)
        {
            try { await _tx.RollbackAsync(ct); }
            finally
            {
                await _tx.DisposeAsync();
                _tx = null;
            }
        }
        db.ChangeTracker.Clear();
    }
}
