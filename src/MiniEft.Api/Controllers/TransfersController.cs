using Microsoft.AspNetCore.Mvc;
using MiniEft.Api.Dtos;
using MiniEft.Application.Transfers;

namespace MiniEft.Api.Controllers;

[ApiController]
[Route("transfers")]
[Produces("application/json")]
public sealed class TransfersController(ITransferService service) : ControllerBase
{
    /// <summary>Move an amount (IQD) between two accounts.</summary>
    /// <remarks>
    /// Requires an <c>Idempotency-Key</c> header. Repeating the same request with the same key returns the original
    /// transfer (200) instead of moving money twice. Insufficient funds is a business outcome, not an HTTP error:
    /// the transfer is stored with status <c>Failed</c> and returned with 201.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromBody] CreateTransferRequest request,
        CancellationToken ct)
    {
        var result = await service.CreateAsync(
            new CreateTransferCommand(
                idempotencyKey ?? string.Empty,
                request.FromAccountId,
                request.ToAccountId,
                request.Amount),
                ct);

        return result.Created
            ? StatusCode(StatusCodes.Status201Created, result.Transfer)
            : Ok(result.Transfer);
    }



    /// <summary>Get the status of a transfer: Pending, Completed, Failed or Reversed.</summary>
    [HttpGet("{transferId:guid}")]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransferDto>> Get(Guid transferId, CancellationToken ct) =>
        await service.GetAsync(transferId, ct);




    /// <summary>Reverse a Completed transfer (money goes back to the sender).</summary>
    [HttpPost("{transferId:guid}/reverse")]
    [ProducesResponseType(typeof(TransferDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransferDto>> Reverse(Guid transferId, CancellationToken ct) =>
        await service.ReverseAsync(transferId, ct);
}
