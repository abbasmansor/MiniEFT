using MiniEft.Domain.Enums;
using MiniEft.Domain.Exceptions;
using MiniEft.Domain.ValueObjects;

namespace MiniEft.Domain.Entities;


public sealed class Transfer
{
    public Guid Id { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public Guid FromAccountId { get; private set; }
    public Guid ToAccountId { get; private set; }
    public const long MinAmount = 1_000;

    public Money Amount { get; private set; }
    public TransferStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset? ReversedAt { get; private set; }

    private Transfer() { } 

    public static Transfer Create(string idempotencyKey, Guid fromAccountId, Guid toAccountId, Money amount, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new InvalidTransferStateException("Idempotency key is required.");
        if (fromAccountId == toAccountId)
            throw new InvalidTransferStateException("Source and destination accounts must be different.");
        if (amount.Amount < MinAmount)
            throw new InvalidAmountException($"Amount must be at least {MinAmount} IQD.");

        return new Transfer
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            FromAccountId = fromAccountId,
            ToAccountId = toAccountId,
            Amount = amount,
            Status = TransferStatus.Pending,
            CreatedAt = now
        };
    }

    public void Complete(DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Pending, "completed");
        Status = TransferStatus.Completed;
        CompletedAt = now;
    }

    public void Fail(string reason, DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Pending, "failed");
        Status = TransferStatus.Failed;
        FailureReason = reason;
        CompletedAt = now;
    }

    public void MarkReversed(DateTimeOffset now)
    {
        EnsureStatus(TransferStatus.Completed, "reversed");
        Status = TransferStatus.Reversed;
        ReversedAt = now;
    }

    private void EnsureStatus(TransferStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidTransferStateException($"Transfer in status {Status} cannot be {action}; expected {expected}.");
    }
}
