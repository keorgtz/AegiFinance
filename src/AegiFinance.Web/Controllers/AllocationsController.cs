using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Allocations.Commands.AutoAllocate;
using AegiFinance.Application.Features.Allocations.Commands.ManualAllocate;
using AegiFinance.Application.Features.Allocations.Commands.Unallocate;
using AegiFinance.Application.Features.Allocations.Queries.GetBillingItemAllocations;
using AegiFinance.Application.Features.Allocations.Queries.GetClientAllocations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AllocationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AllocationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("auto-allocate/{ledgerEntryId:guid}")]
    [Authorize(Policy = "AllocatePayments")]
    public async Task<ActionResult<AllocationResult>> AutoAllocate(Guid ledgerEntryId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new AutoAllocateCommand(ledgerEntryId), cancellationToken));
    }

    [HttpPost("manual")]
    [Authorize(Policy = "AllocatePayments")]
    public async Task<ActionResult<AllocationResult>> ManualAllocate(ManualAllocateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/unallocate")]
    [Authorize(Policy = "UnallocatePayments")]
    public async Task<IActionResult> Unallocate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnallocateCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("client/{clientId:guid}")]
    [HttpGet("/api/clients/{clientId:guid}/allocations")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<List<SubscriptionAllocationDto>>> GetClientAllocations(Guid clientId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientAllocationsQuery(clientId), cancellationToken));
    }

    [HttpGet("billing-item/{billingItemId:guid}")]
    [HttpGet("/api/billing-items/{billingItemId:guid}/allocations")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<List<SubscriptionAllocationDto>>> GetBillingItemAllocations(Guid billingItemId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBillingItemAllocationsQuery(billingItemId), cancellationToken));
    }
}
