using MiniEft.Domain.Exceptions;

namespace MiniEft.Domain.ValueObjects;

public readonly record struct Money
{
    public long Amount { get; }

    private Money(long amount)
    {
        Amount = amount;
    }
    public static Money Zero => new(0);

    public static Money Of(long amount) =>
        amount < 0 ? throw new InvalidAmountException("Amount cannot be negative.") : new Money(amount);

    /// <summary>Strictly positive amount, used for transfers/debits/credits.</summary>
    public static Money Positive(long amount) =>
        amount <= 0 ? throw new InvalidAmountException("Amount must be greater than zero.") : new Money(amount);

    public static Money operator +(Money a, Money b) => Of(checked(a.Amount + b.Amount));

    public static Money operator -(Money a, Money b) =>
        a.Amount < b.Amount ? throw new InvalidAmountException("Result would be negative.") : new Money(a.Amount - b.Amount);

    public static bool operator >(Money a, Money b) => a.Amount > b.Amount;
    public static bool operator <(Money a, Money b) => a.Amount < b.Amount;
    public static bool operator >=(Money a, Money b) => a.Amount >= b.Amount;
    public static bool operator <=(Money a, Money b) => a.Amount <= b.Amount;

    public override string ToString() => $"{Amount} IQD";
}
