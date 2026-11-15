using MiniEft.Application.Abstractions;
using MiniEft.Application.Exceptions;

namespace MiniEft.Application.Accounts;

public sealed class StatementService(IAccountRepository accounts, ILedgerRepository ledger) : IStatementService
{
    public const int MaxPageSize = 200;

    public async Task<StatementDto> GetAsync(
        Guid accountId, DateTimeOffset? from, DateTimeOffset? to, int page, int pageSize, CancellationToken ct)
    {
        if (page < 1) throw new ValidationException("page must be at least 1.");
        if (pageSize is < 1 or > MaxPageSize) throw new ValidationException($"pageSize must be between 1 and {MaxPageSize}.");
        if (from is not null && to is not null && from > to) throw new ValidationException("The start date must be before or equal to the end date.");

        var account = await accounts.GetByIdAsync(accountId, ct) ?? throw new NotFoundException($"Account {accountId} not found.");
        var (items, total) = await ledger.GetByAccountAsync(accountId, from, to, page, pageSize, ct);

        var movements = items
            .Select(e => new StatementEntryDto(e.Id, e.TransferId, e.Type.ToString(), e.Kind.ToString(),
                e.Amount.Amount, e.BalanceAfter.Amount, e.CreatedAt))
            .ToList();

        return new StatementDto(account.Id, account.AccountNumber, account.HolderName, account.Currency,
            account.Balance.Amount, page, pageSize, total, movements);
    }
}
