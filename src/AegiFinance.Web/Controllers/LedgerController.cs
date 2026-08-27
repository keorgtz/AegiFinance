using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Ledger.Commands.RegisterAdjustment;
using AegiFinance.Application.Features.Ledger.Commands.RegisterExpense;
using AegiFinance.Application.Features.Ledger.Commands.RegisterIncome;
using AegiFinance.Application.Features.Ledger.Commands.RegisterTransfer;
using AegiFinance.Application.Features.Ledger.Queries.GetLedgerEntries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LedgerController : ControllerBase
{
    private readonly IMediator _mediator;

    public LedgerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("income")]
    [Authorize(Policy = "CreateLedgerIncome")]
    public async Task<ActionResult<LedgerEntryDto>> RegisterIncome(RegisterIncomeCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("expense")]
    [Authorize(Policy = "CreateLedgerExpense")]
    public async Task<ActionResult<LedgerEntryDto>> RegisterExpense(RegisterExpenseCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("transfer")]
    [Authorize(Policy = "CreateLedgerTransfers")]
    public async Task<ActionResult<TransferGroupDto>> RegisterTransfer(RegisterTransferCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("adjustment")]
    [Authorize(Policy = "CreateLedgerAdjustments")]
    public async Task<ActionResult<LedgerEntryDto>> RegisterAdjustment(RegisterAdjustmentCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<PaginatedList<LedgerEntryListDto>>> GetEntries([FromQuery] GetLedgerEntriesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

}
