using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.ClientUsers.Commands.ActivateClientUser;
using AegiFinance.Application.Features.ClientUsers.Commands.CreateClientUser;
using AegiFinance.Application.Features.ClientUsers.Commands.DeactivateClientUser;
using AegiFinance.Application.Features.ClientUsers.Commands.DeleteClientUser;
using AegiFinance.Application.Features.ClientUsers.Commands.SetClientUserPin;
using AegiFinance.Application.Features.ClientUsers.Commands.UpdateClientUser;
using AegiFinance.Application.Features.ClientUsers.Queries.GetClientUserById;
using AegiFinance.Application.Features.ClientUsers.Queries.GetClientUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/client-users")]
[Authorize]
public class ClientUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ViewClientUsers")]
    public async Task<ActionResult<List<ClientUserDto>>> GetAll([FromQuery] Guid? clientId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientUsersQuery { ClientId = clientId }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ViewClientUsers")]
    public async Task<ActionResult<ClientUserDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClientUserByIdQuery(id), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<ClientUserDto>> Create(CreateClientUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<ActionResult<ClientUserDto>> Update(Guid id, UpdateClientUserCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClientUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateClientUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateClientUserCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/set-pin")]
    [Authorize(Policy = "ManageUsers")]
    public async Task<IActionResult> SetPin(Guid id, SetClientUserPinCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
