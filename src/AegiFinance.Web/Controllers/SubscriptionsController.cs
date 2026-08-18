using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Subscriptions.Commands.CancelSubscription;
using AegiFinance.Application.Features.Subscriptions.Commands.ChangeSubscriptionPrice;
using AegiFinance.Application.Features.Subscriptions.Commands.CreateSubscription;
using AegiFinance.Application.Features.Subscriptions.Commands.DeleteSubscription;
using AegiFinance.Application.Features.Subscriptions.Commands.ReactivateSubscription;
using AegiFinance.Application.Features.Subscriptions.Commands.RenewSubscription;
using AegiFinance.Application.Features.Subscriptions.Commands.SuspendSubscription;
using AegiFinance.Application.Features.Subscriptions.Commands.UpdateSubscription;
using AegiFinance.Application.Features.Subscriptions.Queries.GetClientSubscriptions;
using AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionById;
using AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionChangeLog;
using AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptionPriceHistory;
using AegiFinance.Application.Features.Subscriptions.Queries.GetSubscriptions;
using AegiFinance.Application.Features.SubscriptionPermissions.Commands.CreateSubscriptionPermission;
using AegiFinance.Application.Features.SubscriptionPermissions.Commands.DeleteSubscriptionPermission;
using AegiFinance.Application.Features.SubscriptionPermissions.Queries.GetSubscriptionPermissionsBySubscription;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ViewSubscriptions")]
    public async Task<ActionResult<PaginatedList<SubscriptionListDto>>> GetAll([FromQuery] GetSubscriptionsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ViewSubscriptions")]
    public async Task<ActionResult<SubscriptionDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetSubscriptionByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "CreateSubscriptions")]
    public async Task<ActionResult<SubscriptionDto>> Create(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "UpdateSubscriptions")]
    public async Task<ActionResult<SubscriptionDto>> Update(Guid id, UpdateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DeleteSubscriptions")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSubscriptionCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/suspend")]
    [Authorize(Policy = "ManageSubscriptionLifecycle")]
    public async Task<IActionResult> Suspend(Guid id, SuspendSubscriptionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Policy = "ManageSubscriptionLifecycle")]
    public async Task<IActionResult> Reactivate(Guid id, ReactivateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = "ManageSubscriptionLifecycle")]
    public async Task<IActionResult> Cancel(Guid id, CancelSubscriptionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/renew")]
    [Authorize(Policy = "ManageSubscriptionLifecycle")]
    public async Task<IActionResult> Renew(Guid id, RenewSubscriptionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/change-price")]
    [Authorize(Policy = "UpdateSubscriptions")]
    public async Task<ActionResult<SubscriptionDto>> ChangePrice(Guid id, ChangeSubscriptionPriceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("{id:guid}/price-history")]
    [Authorize(Policy = "ViewSubscriptions")]
    public async Task<ActionResult<List<SubscriptionPriceHistoryDto>>> GetPriceHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetSubscriptionPriceHistoryQuery(id), cancellationToken));
    }

    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = "ViewSubscriptions")]
    public async Task<ActionResult<List<SubscriptionChangeLogDto>>> GetHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetSubscriptionChangeLogQuery(id), cancellationToken));
    }

    [HttpGet("by-client/{clientId:guid}")]
    [Authorize(Policy = "ViewSubscriptions")]
    public async Task<ActionResult<List<SubscriptionListDto>>> GetByClient(Guid clientId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientSubscriptionsQuery(clientId), cancellationToken));
    }

    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<List<SubscriptionPermissionDto>>> GetPermissions(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetSubscriptionPermissionsBySubscriptionQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/permissions")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<SubscriptionPermissionDto>> CreatePermission(Guid id, CreateSubscriptionPermissionCommand command, CancellationToken cancellationToken)
    {
        command.SubscriptionId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> DeletePermission(Guid id, Guid permissionId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSubscriptionPermissionCommand(permissionId), cancellationToken);
        return NoContent();
    }
}
