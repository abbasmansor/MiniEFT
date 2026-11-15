using MiniEft.Domain.Exceptions;
using MiniEft.Domain.ValueObjects;

namespace MiniEft.Domain.Entities;


public sealed class Account
{
    public const string DefaultCurrency = "IQD";

    public Guid Id { get; private set; }
    public string AccountNumber { get; private set; } = default!;
    public string HolderName { get; private set; } = default!;
    public string Currency { get; private set; } = DefaultCurrency;
    public Money Balance { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Account() { } // EF Core

    public static Account Open(Guid id, string accountNumber, string holderName, Money openingBalance, DateTimeOffset now) =>
        new()
        {
            Id = id,
            AccountNumber = accountNumber,
            HolderName = holderName,
            Balance = openingBalance,
            CreatedAt = now
        };

    public void Debit(Money amount)
    {
        if (amount.Amount <= 0) throw new InvalidAmountException("Amount must be greater than zero.");
        if (Balance < amount) throw new InsufficientFundsException();
        Balance -= amount;
    }

    public void Credit(Money amount)
    {
        if (amount.Amount <= 0) throw new InvalidAmountException("Amount must be greater than zero.");
        Balance += amount;
    }
}
