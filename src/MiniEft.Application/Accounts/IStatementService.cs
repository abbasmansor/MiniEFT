namespace MiniEft.Application.Accounts;

public interface IStatementService
{
    Task<StatementDto> GetAsync(
        Guid accountId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int page,
        int pageSize,
        CancellationToken ct);
}
