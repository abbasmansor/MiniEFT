using MiniEft.Application.Abstractions;
using MiniEft.Application.Exceptions;
using MiniEft.Domain.Entities;
using MiniEft.Domain.Enums;
using MiniEft.Domain.Exceptions;
using MiniEft.Domain.ValueObjects;

namespace MiniEft.Application.Transfers;

public sealed class TransferService(
    IAccountRepository accounts,
    ITransferRepository transfers,
    ILedgerRepository ledger,
    IUnitOfWork uow,
    IClock clock) : ITransferService
{
    public async Task<CreateTransferResult> CreateAsync(CreateTransferCommand cmd, CancellationToken ct)
    {
        Validate(cmd);

        // check Idempotency key
        var existing = await transfers.GetByIdempotencyKeyAsync(cmd.IdempotencyKey, ct);
        if (existing is not null) return Replay(existing, cmd);

        // check Both accounts if they exist
        if (!await accounts.ExistsAsync(cmd.FromAccountId, ct)) throw new NotFoundException($"FromAccountId {cmd.FromAccountId} not found.");
        if (!await accounts.ExistsAsync(cmd.ToAccountId, ct)) throw new NotFoundException($"ToAccountId {cmd.ToAccountId} not found.");

        var transfer = Transfer.Create(cmd.IdempotencyKey, cmd.FromAccountId, cmd.ToAccountId, Money.Positive(cmd.Amount), clock.UtcNow);
        await transfers.AddAsync(transfer, ct);
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DuplicateKeyException)
        {
            // Two concurrent requests with the same key: the loser returns the winner's transfer.
            var winner = await transfers.GetByIdempotencyKeyAsync(cmd.IdempotencyKey, ct)
                         ?? throw new ConflictException("Idempotency key conflict.");
            return Replay(winner, cmd);
        }

        // Move the money
        await uow.BeginTransactionAsync(ct);
        try
        {
            var locked = await accounts.GetForUpdateAsync(cmd.FromAccountId, cmd.ToAccountId, ct);
            var from = locked[cmd.FromAccountId];
            var to = locked[cmd.ToAccountId];
            var amount = transfer.Amount;
            var now = clock.UtcNow;

            try
            {
                from.Debit(amount);
                to.Credit(amount);
                await ledger.AddRangeAsync(
                [
                    LedgerEntry.Debit(from.Id, transfer.Id, EntryKind.Transfer, amount, from.Balance, now),
                    LedgerEntry.Credit(to.Id, transfer.Id, EntryKind.Transfer, amount, to.Balance, now)
                ], ct);
                transfer.Complete(now);
            }
            catch (InsufficientFundsException)
            {
                transfer.Fail("INSUFFICIENT_FUNDS", now);
            }

            await uow.SaveChangesAsync(ct);
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            await MarkFailedAfterErrorAsync(transfer.Id, ct);
            throw;
        }

        return new CreateTransferResult(TransferDto.From(transfer), Created: true);
    }

    public async Task<TransferDto> GetAsync(Guid id, CancellationToken ct)
    {
        var transfer = await transfers.GetByIdAsync(id, ct) ?? throw new NotFoundException($"Transfer {id} not found.");
        return TransferDto.From(transfer);
    }

    public async Task<TransferDto> ReverseAsync(Guid id, CancellationToken ct)
    {
        await uow.BeginTransactionAsync(ct);
        try
        {
            // Lock the transfer first so two concurrent reversals serialize; the state check below runs under the lock.
            var transfer = await transfers.GetByIdForUpdateAsync(id, ct) ?? throw new NotFoundException($"Transfer {id} not found.");
            if (transfer.Status != TransferStatus.Completed)
                throw new ConflictException($"Only Completed transfers can be reversed (current status: {transfer.Status}).");

            var locked = await accounts.GetForUpdateAsync(transfer.FromAccountId, transfer.ToAccountId, ct);
            var from = locked[transfer.FromAccountId];
            var to = locked[transfer.ToAccountId];
            var amount = transfer.Amount;
            var now = clock.UtcNow;

            try
            {
                to.Debit(amount);
            }
            catch (InsufficientFundsException)
            {
                throw new ConflictException("Receiver balance is too low to reverse this transfer.");
            }
            from.Credit(amount);

            await ledger.AddRangeAsync(
            [
                LedgerEntry.Debit(to.Id, transfer.Id, EntryKind.Reversal, amount, to.Balance, now),
                LedgerEntry.Credit(from.Id, transfer.Id, EntryKind.Reversal, amount, from.Balance, now)
            ], ct);
            transfer.MarkReversed(now);

            await uow.SaveChangesAsync(ct);
            await uow.CommitAsync(ct);
            return TransferDto.From(transfer);
        }
        catch
        {
            await uow.RollbackAsync(ct);
            throw;
        }
    }

    private static void Validate(CreateTransferCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.IdempotencyKey)) throw new ValidationException("Idempotency-Key header is required.");
        if (cmd.IdempotencyKey.Length > 100) throw new ValidationException("Idempotency-Key must be at most 100 characters.");
        if (cmd.Amount < Transfer.MinAmount) throw new ValidationException($"Amount must be at least {Transfer.MinAmount} IQD.");
        if (cmd.FromAccountId == Guid.Empty || cmd.ToAccountId == Guid.Empty) throw new ValidationException("Both account ids are required.");
        if (cmd.FromAccountId == cmd.ToAccountId) throw new ValidationException("Source and destination accounts must be different.");
    }

    private static CreateTransferResult Replay(Transfer existing, CreateTransferCommand cmd)
    {
        var sameRequest = existing.FromAccountId == cmd.FromAccountId
                          && existing.ToAccountId == cmd.ToAccountId
                          && existing.Amount.Amount == cmd.Amount;
        if (!sameRequest) throw new ConflictException("Idempotency-Key was already used with a different request.");
        return new CreateTransferResult(TransferDto.From(existing), Created: false);
    }

    private async Task MarkFailedAfterErrorAsync(Guid transferId, CancellationToken ct)
    {
        try
        {
            await uow.BeginTransactionAsync(ct);
            var transfer = await transfers.GetByIdForUpdateAsync(transferId, ct);
            if (transfer is { Status: TransferStatus.Pending })
            {
                transfer.Fail("SYSTEM_ERROR", clock.UtcNow);
                await uow.SaveChangesAsync(ct);
            }
            await uow.CommitAsync(ct);
        }
        catch
        {
            await uow.RollbackAsync(ct);
        }
    }
}
