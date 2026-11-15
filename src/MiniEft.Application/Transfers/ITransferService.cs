namespace MiniEft.Application.Transfers;

public interface ITransferService
{
    Task<CreateTransferResult> CreateAsync(CreateTransferCommand cmd, CancellationToken ct);

    Task<TransferDto> GetAsync(Guid id, CancellationToken ct);

    Task<TransferDto> ReverseAsync(Guid id, CancellationToken ct);
}
