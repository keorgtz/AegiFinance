using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Permissions.Commands.CreatePermission;
using AegiFinance.Application.Features.Permissions.Commands.DeletePermission;
using AegiFinance.Application.Features.Permissions.Commands.UpdatePermission;
using AegiFinance.Application.Features.Permissions.Queries.GetPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult<List<PermissionDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetPermissionsQuery(), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult<PermissionDto>> Create(CreatePermissionCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult<PermissionDto>> Update(Guid id, UpdatePermissionCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeletePermissionCommand(id), cancellationToken);
        return NoContent();
    }
}
