using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Allocations.Commands.AutoAllocate;
using AegiFinance.Application.Features.Allocations.Commands.ManualAllocate;
using AegiFinance.Application.Features.Allocations.Commands.Unallocate;
using AegiFinance.Application.Features.Allocations;
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

    [HttpGet("applications")]
    [Authorize(Policy = "ViewPaymentApplications")]
    public async Task<ActionResult<IReadOnlyList<PaymentApplicationDto>>> Applications([FromQuery] GetPaymentApplicationsQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("applications/{id:guid}")]
    [Authorize(Policy = "ViewPaymentApplications")]
    public async Task<ActionResult<PaymentApplicationDto>> Application(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetPaymentApplicationQuery(id), cancellationToken));

    [HttpGet("applications/{id:guid}/receipt")]
    [Authorize(Policy = "ViewPaymentReceipts")]
    public async Task<ActionResult<PaymentApplicationDto>> Receipt(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetPaymentApplicationQuery(id), cancellationToken));

    [HttpPost("applications/auto")]
    [Authorize(Policy = "ApplyPayments")]
    public async Task<ActionResult<AllocationResult>> ApplyAutomatically(ApplyPaymentsAutomaticallyCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));

    [HttpPost("applications/manual")]
    [Authorize(Policy = "ApplyPayments")]
    public async Task<ActionResult<AllocationResult>> ApplyManually(ApplyPaymentsManuallyCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));

    [HttpPost("applications/{id:guid}/reverse")]
    [Authorize(Policy = "ReversePaymentApplications")]
    public async Task<IActionResult> ReverseApplication(Guid id, [FromBody] PaymentApplicationReasonRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReversePaymentApplicationCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("applications/{id:guid}/reapply")]
    [Authorize(Policy = "ReapplyPayments")]
    public async Task<ActionResult<AllocationResult>> ReapplyApplication(Guid id, ReapplyPaymentApplicationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("settings")]
    [Authorize(Policy = "ViewPaymentApplications")]
    public async Task<ActionResult<PaymentApplicationSettingsDto>> Settings(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetPaymentApplicationSettingsQuery(), cancellationToken));

    [HttpPut("settings")]
    [Authorize(Policy = "ManagePaymentApplicationSettings")]
    public async Task<ActionResult<PaymentApplicationSettingsDto>> UpdateSettings(UpdatePaymentApplicationSettingsCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));
}

public sealed class PaymentApplicationReasonRequest { public string Reason { get; set; } = string.Empty; }
