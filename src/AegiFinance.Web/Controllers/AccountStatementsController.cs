using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientFinancialSummary;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientMovements;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountStatementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountStatementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("client/{clientId:guid}")]
    public async Task<ActionResult<AccountStatementDto>> GetStatement(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientStatementQuery
        {
            ClientId = clientId,
            From = from,
            To = to,
            Currency = currency
        }, cancellationToken));
    }

    [HttpGet("client/{clientId:guid}/summary")]
    public async Task<ActionResult<FinancialSummaryDto>> GetSummary(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientFinancialSummaryQuery
        {
            ClientId = clientId,
            From = from,
            To = to,
            Currency = currency
        }, cancellationToken));
    }

    [HttpGet("client/{clientId:guid}/movements")]
    public async Task<ActionResult<List<AccountStatementItemDto>>> GetMovements(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientMovementsQuery
        {
            ClientId = clientId,
            From = from,
            To = to,
            Currency = currency
        }, cancellationToken));
    }
}
