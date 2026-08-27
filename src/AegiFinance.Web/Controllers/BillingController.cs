using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;
using AegiFinance.Application.Features.Billing.Commands.AddBillingAdjustment;
using AegiFinance.Application.Features.Billing.Commands.CreateManualCharge;
using AegiFinance.Application.Features.Billing.Commands.CreatePaymentPromise;
using AegiFinance.Application.Features.Billing.Commands.UpdatePaymentPromiseStatus;
using AegiFinance.Application.Features.Billing.Commands.CloseBillingCycle;
using AegiFinance.Application.Features.Billing.Commands.GenerateBilling;
using AegiFinance.Application.Features.Billing.Commands.GenerateBillingForSubscription;
using AegiFinance.Application.Features.Billing.Commands.ReprocessBillingCycle;
using AegiFinance.Application.Features.Billing.Queries.GetBillingCycles;
using AegiFinance.Application.Features.Billing.Queries.GetBillingGenerationLogs;
using AegiFinance.Application.Features.Billing.Queries.GetBillingItems;
using AegiFinance.Application.Features.Billing.Queries.GetReceivablesAging;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BillingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("generate")]
    [Authorize(Policy = "GenerateBilling")]
    public async Task<ActionResult<BillingGenerationResult>> Generate(GenerateBillingCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("generate-for-subscription")]
    [Authorize(Policy = "GenerateBilling")]
    public async Task<ActionResult<BillingGenerationResult>> GenerateForSubscription(GenerateBillingForSubscriptionCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("items/manual")]
    [Authorize(Policy = "CreateBillingItems")]
    public async Task<ActionResult<BillingItemListDto>> CreateManualCharge(CreateManualChargeCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPost("items/{itemId:guid}/adjustments")]
    [Authorize(Policy = "AdjustBillingItems")]
    public async Task<ActionResult<BillingAdjustmentDto>> AddAdjustment(Guid itemId, AddBillingAdjustmentCommand command, CancellationToken cancellationToken)
    {
        command.BillingItemId = itemId;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("items/{itemId:guid}/payment-promises")]
    [Authorize(Policy = "ManagePaymentPromises")]
    public async Task<ActionResult<PaymentPromiseDto>> CreatePromise(Guid itemId, CreatePaymentPromiseCommand command, CancellationToken cancellationToken)
    {
        command.BillingItemId = itemId;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("payment-promises/{promiseId:guid}/status")]
    [Authorize(Policy = "ManagePaymentPromises")]
    public async Task<IActionResult> UpdatePromise(Guid promiseId, UpdatePaymentPromiseStatusCommand command, CancellationToken cancellationToken)
    {
        command.PaymentPromiseId = promiseId;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("reprocess/{cycleId:guid}")]
    [Authorize(Policy = "ReprocessBilling")]
    public async Task<ActionResult<BillingGenerationResult>> Reprocess(Guid cycleId, [FromQuery] bool onlyPending = true, CancellationToken cancellationToken = default)
    {
        var command = new ReprocessBillingCycleCommand
        {
            BillingCycleId = cycleId,
            OnlyPending = onlyPending
        };

        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("cancel-item/{itemId:guid}")]
    [Authorize(Policy = "CancelBillingItems")]
    public async Task<IActionResult> CancelItem(Guid itemId, CancelBillingItemCommand command, CancellationToken cancellationToken)
    {
        command.BillingItemId = itemId;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("close-cycle/{cycleId:guid}")]
    [Authorize(Policy = "CloseBillingCycles")]
    public async Task<IActionResult> CloseCycle(Guid cycleId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CloseBillingCycleCommand { BillingCycleId = cycleId }, cancellationToken);
        return NoContent();
    }

    [HttpGet("cycles")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<List<BillingCycleDto>>> GetCycles([FromQuery] GetBillingCyclesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("cycles/{id:guid}/items")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<List<BillingItemListDto>>> GetItemsByCycle(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBillingItemsQuery { BillingCycleId = id }, cancellationToken));
    }

    [HttpGet("items")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<PaginatedList<BillingItemListDto>>> GetItems([FromQuery] GetBillingItemsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("receivables/aging")]
    [Authorize(Policy = "ViewReceivables")]
    public async Task<ActionResult<ReceivablesAgingDto>> GetAging([FromQuery] GetReceivablesAgingQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("generation-logs")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<List<BillingGenerationLogDto>>> GetLogs([FromQuery] GetBillingGenerationLogsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }
}
