namespace MiniEft.Domain.Exceptions;

public abstract class DomainException(string message) : Exception(message);

public sealed class InvalidAmountException(string message) : DomainException(message);

public sealed class InsufficientFundsException(string message = "Insufficient funds.") : DomainException(message);

public sealed class InvalidTransferStateException(string message) : DomainException(message);
