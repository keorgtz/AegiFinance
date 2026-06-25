using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.SubscriptionPermissions.Commands.CreateSubscriptionPermission;
using AegiFinance.Application.Features.SubscriptionPermissions.Commands.DeleteSubscriptionPermission;
using AegiFinance.Application.Features.SubscriptionPermissions.Queries.GetSubscriptionPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/subscription-permissions")]
[Authorize]
public class SubscriptionPermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionPermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubscriptionPermissionDto>>> GetAll([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetSubscriptionPermissionsQuery(userId), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<SubscriptionPermissionDto>> Create(CreateSubscriptionPermissionCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSubscriptionPermissionCommand(id), cancellationToken);
        return NoContent();
    }
}
