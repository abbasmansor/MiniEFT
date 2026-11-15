namespace MiniEft.Application.Accounts;

public sealed record StatementDto(
    Guid AccountId,
    string AccountNumber,
    string HolderName,
    string Currency,
    long Balance,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<StatementEntryDto> Movements);

public sealed record StatementEntryDto(
    long Id,
    Guid TransferId,
    string Type,
    string Kind,
    long Amount,
    long BalanceAfter,
    DateTimeOffset CreatedAt);
