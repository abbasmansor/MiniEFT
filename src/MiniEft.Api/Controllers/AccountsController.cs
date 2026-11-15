using Microsoft.AspNetCore.Mvc;
using MiniEft.Application.Accounts;

namespace MiniEft.Api.Controllers;

[ApiController]
[Route("accounts")]
[Produces("application/json")]
public sealed class AccountsController(IStatementService service) : ControllerBase
{
    /// <summary>Current balance and ledger movements (newest first, paginated).</summary>
    [HttpGet("{accountId:guid}/statement")]
    [ProducesResponseType(typeof(StatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatementDto>> Statement(
        Guid accountId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default) =>
        await service.GetAsync(accountId, from, to, page, pageSize, ct);
}
