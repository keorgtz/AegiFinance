using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Billing.Commands.CancelBillingItem;
using AegiFinance.Application.Features.Billing.Commands.CloseBillingCycle;
using AegiFinance.Application.Features.Billing.Commands.GenerateBilling;
using AegiFinance.Application.Features.Billing.Commands.GenerateBillingForSubscription;
using AegiFinance.Application.Features.Billing.Commands.ReprocessBillingCycle;
using AegiFinance.Application.Features.Billing.Queries.GetBillingCycles;
using AegiFinance.Application.Features.Billing.Queries.GetBillingGenerationLogs;
using AegiFinance.Application.Features.Billing.Queries.GetBillingItems;
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
    public async Task<ActionResult<BillingGenerationResult>> Generate(GenerateBillingCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("generate-for-subscription")]
    public async Task<ActionResult<BillingGenerationResult>> GenerateForSubscription(GenerateBillingForSubscriptionCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("reprocess/{cycleId:guid}")]
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
    public async Task<IActionResult> CancelItem(Guid itemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelBillingItemCommand { BillingItemId = itemId }, cancellationToken);
        return NoContent();
    }

    [HttpPost("close-cycle/{cycleId:guid}")]
    public async Task<IActionResult> CloseCycle(Guid cycleId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CloseBillingCycleCommand { BillingCycleId = cycleId }, cancellationToken);
        return NoContent();
    }

    [HttpGet("cycles")]
    public async Task<ActionResult<List<BillingCycleDto>>> GetCycles([FromQuery] GetBillingCyclesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("cycles/{id:guid}/items")]
    public async Task<ActionResult<List<BillingItemListDto>>> GetItemsByCycle(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBillingItemsQuery { BillingCycleId = id }, cancellationToken));
    }

    [HttpGet("items")]
    public async Task<ActionResult<PaginatedList<BillingItemListDto>>> GetItems([FromQuery] GetBillingItemsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("generation-logs")]
    public async Task<ActionResult<List<BillingGenerationLogDto>>> GetLogs([FromQuery] GetBillingGenerationLogsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }
}
