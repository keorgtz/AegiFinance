using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Users.Commands.ActivateUser;
using AegiFinance.Application.Features.Users.Commands.AssignRolesToUser;
using AegiFinance.Application.Features.Users.Commands.CreateUser;
using AegiFinance.Application.Features.Users.Commands.DeactivateUser;
using AegiFinance.Application.Features.Users.Commands.DeleteUser;
using AegiFinance.Application.Features.Users.Commands.RemoveUserPermission;
using AegiFinance.Application.Features.Users.Commands.SetUserPermission;
using AegiFinance.Application.Features.Users.Commands.UpdateUser;
using AegiFinance.Application.Features.Users.Commands.RevokeUserSession;
using AegiFinance.Application.Features.Users.Commands.RevokeAllUserSessions;
using AegiFinance.Application.Features.Users.Commands.UnlockUser;
using AegiFinance.Application.Features.Users.Commands.ResetUserPassword;
using AegiFinance.Application.Features.Users.Queries.GetUserById;
using AegiFinance.Application.Features.Users.Queries.GetUsers;
using AegiFinance.Application.Features.Users.Queries.GetUserSessions;
using AegiFinance.Application.Features.Users.Queries.GetUserPermissionOverrides;
using AegiFinance.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<PaginatedList<UserDto>>> GetAll([FromQuery] GetUsersQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<UserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery { Id = id }, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<UserDto>> Create(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> AssignRoles(Guid id, AssignRolesToUserCommand command, CancellationToken cancellationToken)
    {
        command.UserId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/permissions")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> SetPermission(Guid id, SetUserPermissionCommand command, CancellationToken cancellationToken)
    {
        command.UserId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> RemovePermission(Guid id, Guid permissionId, [FromQuery] Guid? clientId, [FromQuery] Guid? subscriptionId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveUserPermissionCommand
        {
            UserId = id, PermissionId = permissionId, ClientId = clientId, SubscriptionId = subscriptionId
        }, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/permissions")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<List<UserPermissionOverrideDto>>> GetPermissionOverrides(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUserPermissionOverridesQuery(id), cancellationToken));

    [HttpGet("{id:guid}/sessions")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<List<UserSessionDto>>> GetSessions(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetUserSessionsQuery(id), cancellationToken));

    [HttpDelete("{id:guid}/sessions/{sessionId:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> RevokeSession(Guid id, Guid sessionId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeUserSessionCommand(id, sessionId, User.GetUserId()), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/sessions")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> RevokeAllSessions(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RevokeAllUserSessionsCommand(id, User.GetUserId()), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unlock")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnlockUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reset-password")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetUserPasswordCommand command, CancellationToken cancellationToken)
    {
        command.UserId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
