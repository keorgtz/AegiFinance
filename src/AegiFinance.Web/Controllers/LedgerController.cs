using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Ledger.Commands.ReconcileLedgerEntry;
using AegiFinance.Application.Features.Ledger.Commands.RegisterAdjustment;
using AegiFinance.Application.Features.Ledger.Commands.RegisterExpense;
using AegiFinance.Application.Features.Ledger.Commands.RegisterIncome;
using AegiFinance.Application.Features.Ledger.Commands.RegisterTransfer;
using AegiFinance.Application.Features.Ledger.Commands.UnreconcileLedgerEntry;
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
    [Authorize(Policy = "ManageBilling")]
    public async Task<ActionResult<LedgerEntryDto>> RegisterIncome(RegisterIncomeCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("expense")]
    [Authorize(Policy = "ManageBilling")]
    public async Task<ActionResult<LedgerEntryDto>> RegisterExpense(RegisterExpenseCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("transfer")]
    [Authorize(Policy = "ManageBilling")]
    public async Task<ActionResult<TransferGroupDto>> RegisterTransfer(RegisterTransferCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("adjustment")]
    [Authorize(Policy = "ManageBilling")]
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

    [HttpPost("{id:guid}/reconcile")]
    [Authorize(Policy = "ManageReconciliation")]
    public async Task<IActionResult> Reconcile(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReconcileLedgerEntryCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unreconcile")]
    [Authorize(Policy = "ManageReconciliation")]
    public async Task<IActionResult> Unreconcile(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnreconcileLedgerEntryCommand(id), cancellationToken);
        return NoContent();
    }
}
