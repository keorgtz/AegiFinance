using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientFinancialSummary;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientMovements;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;
using AegiFinance.Application.Features.Clients.Commands.ActivateClient;
using AegiFinance.Application.Features.Clients.Commands.AddClientContact;
using AegiFinance.Application.Features.Clients.Commands.AddClientNote;
using AegiFinance.Application.Features.Clients.Commands.AssignTagsToClient;
using AegiFinance.Application.Features.Clients.Commands.CreateClient;
using AegiFinance.Application.Features.Clients.Commands.DeactivateClient;
using AegiFinance.Application.Features.Clients.Commands.DeleteClient;
using AegiFinance.Application.Features.Clients.Commands.DeleteClientContact;
using AegiFinance.Application.Features.Clients.Commands.SetPrimaryContact;
using AegiFinance.Application.Features.Clients.Commands.UpdateClient;
using AegiFinance.Application.Features.Clients.Commands.UpdateClientContact;
using AegiFinance.Application.Features.Clients.Queries.GetClientById;
using AegiFinance.Application.Features.Clients.Queries.GetClientContacts;
using AegiFinance.Application.Features.Clients.Queries.GetClientNotes;
using AegiFinance.Application.Features.Clients.Queries.GetClients;
using AegiFinance.Application.Features.Subscriptions.Queries.GetClientSubscriptions;
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
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<PaginatedList<ClientListDto>>> GetAll([FromQuery] GetClientsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<ClientDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientByIdQuery(id), cancellationToken));
    }

    [HttpGet("{id:guid}/contacts")]
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<List<ClientContactDto>>> GetContacts(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientContactsQuery(id), cancellationToken));
    }

    [HttpGet("{id:guid}/notes")]
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<List<ClientNoteDto>>> GetNotes(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientNotesQuery(id), cancellationToken));
    }

    [HttpGet("{id:guid}/subscriptions")]
    [Authorize(Policy = "ViewSubscriptions")]
    public async Task<ActionResult<List<SubscriptionListDto>>> GetSubscriptions(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientSubscriptionsQuery(id), cancellationToken));
    }

    [HttpGet("{id:guid}/statement")]
    [Authorize(Policy = "ViewPayments")]
    public async Task<ActionResult<AccountStatementDto>> GetStatement(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientStatementQuery { ClientId = id, From = from, To = to, Currency = currency }, cancellationToken));
    }

    [HttpGet("{id:guid}/financial-summary")]
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<FinancialSummaryDto>> GetFinancialSummary(Guid id, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientFinancialSummaryQuery { ClientId = id, Currency = currency }, cancellationToken));
    }

    [HttpGet("{id:guid}/movements")]
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<List<AccountStatementItemDto>>> GetMovements(Guid id, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientMovementsQuery { ClientId = id, Currency = currency }, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "CreateClients")]
    public async Task<ActionResult<ClientDto>> Create(CreateClientCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, UpdateClientCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DeleteClients")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClientCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateClientCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateClientCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/tags")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<IActionResult> AssignTags(Guid id, AssignTagsToClientCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/notes")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<ActionResult<ClientNoteDto>> AddNote(Guid id, AddClientNoteCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("{id:guid}/contacts")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<ActionResult<ClientContactDto>> AddContact(Guid id, AddClientContactCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}/contacts/{contactId:guid}")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<ActionResult<ClientContactDto>> UpdateContact(Guid id, Guid contactId, UpdateClientContactCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = id;
        command.ContactId = contactId;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}/contacts/{contactId:guid}")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<IActionResult> DeleteContact(Guid id, Guid contactId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClientContactCommand(id, contactId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/contacts/{contactId:guid}/primary")]
    [Authorize(Policy = "UpdateClients")]
    public async Task<IActionResult> SetPrimaryContact(Guid id, Guid contactId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SetPrimaryContactCommand(id, contactId), cancellationToken);
        return NoContent();
    }
}
