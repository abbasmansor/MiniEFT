namespace MiniEft.Api.Dtos;

public sealed record CreateTransferRequest(Guid FromAccountId, Guid ToAccountId, long Amount);
