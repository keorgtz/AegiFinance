using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Clients.Commands.ActivateClient;
using AegiFinance.Application.Features.Clients.Commands.AddClientContact;
using AegiFinance.Application.Features.Clients.Commands.AddClientNote;
using AegiFinance.Application.Features.Clients.Commands.AssignTagsToClient;
using AegiFinance.Application.Features.Clients.Commands.CreateClient;
using AegiFinance.Application.Features.Clients.Commands.DeactivateClient;
using AegiFinance.Application.Features.Clients.Commands.DeleteClient;
using AegiFinance.Application.Features.Clients.Commands.SetPrimaryContact;
using AegiFinance.Application.Features.Clients.Commands.UpdateClient;
using AegiFinance.Application.Features.Clients.Queries.GetClientById;
using AegiFinance.Application.Features.Clients.Queries.GetClients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ClientListDto>>> GetAll([FromQuery] GetClientsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClientDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(CreateClientCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, UpdateClientCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClientCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateClientCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateClientCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/tags")]
    public async Task<IActionResult> AssignTags(Guid id, AssignTagsToClientCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<ClientNoteDto>> AddNote(Guid id, AddClientNoteCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/contacts")]
    public async Task<ActionResult<ClientContactDto>> AddContact(Guid id, AddClientContactCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/contacts/{contactId:guid}/primary")]
    public async Task<IActionResult> SetPrimaryContact(Guid id, Guid contactId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetPrimaryContactCommand(id, contactId), cancellationToken);
        return NoContent();
    }
}
