using MiniEft.Domain.Enums;
using MiniEft.Domain.ValueObjects;

namespace MiniEft.Domain.Entities;

public sealed class LedgerEntry
{
    public long Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid TransferId { get; private set; }
    public EntryType Type { get; private set; }
    public EntryKind Kind { get; private set; }
    public Money Amount { get; private set; }
    public Money BalanceAfter { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private LedgerEntry() { }

    public static LedgerEntry Debit(Guid accountId, Guid transferId, EntryKind kind, Money amount, Money balanceAfter, DateTimeOffset now) =>
        new()
        {
            AccountId = accountId,
            TransferId = transferId,
            Type = EntryType.Debit,
            Kind = kind,
            Amount = amount,
            BalanceAfter = balanceAfter,
            CreatedAt = now
        };

    public static LedgerEntry Credit(Guid accountId, Guid transferId, EntryKind kind, Money amount, Money balanceAfter, DateTimeOffset now) =>
        new()
        {
            AccountId = accountId,
            TransferId = transferId,
            Type = EntryType.Credit,
            Kind = kind,
            Amount = amount,
            BalanceAfter = balanceAfter,
            CreatedAt = now
        };

}
