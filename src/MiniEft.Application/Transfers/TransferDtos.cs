using MiniEft.Domain.Entities;

namespace MiniEft.Application.Transfers;

public sealed record CreateTransferCommand(
    string IdempotencyKey,
    Guid FromAccountId,
    Guid ToAccountId,
    long Amount
    );

public sealed record CreateTransferResult(TransferDto Transfer, bool Created);

public sealed record TransferDto(
    Guid Id,
    Guid FromAccountId,
    Guid ToAccountId,
    long Amount,
    string Currency,
    string Status,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt,
    DateTimeOffset? ReversedAt)
{
public static TransferDto From(Transfer t) => new(
    t.Id,
    t.FromAccountId,
    t.ToAccountId,
    t.Amount.Amount,
    Account.DefaultCurrency,
    t.Status.ToString(),
    t.FailureReason,
    t.CreatedAt,
    t.CompletedAt,
    t.ReversedAt );
}
