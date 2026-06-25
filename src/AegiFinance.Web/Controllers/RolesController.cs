using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Roles.Commands.AssignPermissionsToRole;
using AegiFinance.Application.Features.Roles.Commands.CreateRole;
using AegiFinance.Application.Features.Roles.Commands.DeleteRole;
using AegiFinance.Application.Features.Roles.Commands.UpdateRole;
using AegiFinance.Application.Features.Roles.Queries.GetRoleById;
using AegiFinance.Application.Features.Roles.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetRolesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRoleByIdQuery(id), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult<RoleDto>> Create(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<ActionResult<RoleDto>> Update(Guid id, UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteRoleCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/permissions")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> AssignPermissions(Guid id, AssignPermissionsToRoleCommand command, CancellationToken cancellationToken)
    {
        command.RoleId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
