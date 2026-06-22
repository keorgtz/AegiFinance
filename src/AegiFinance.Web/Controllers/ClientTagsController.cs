using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.ClientTags.Commands.CreateClientTag;
using AegiFinance.Application.Features.ClientTags.Commands.DeleteClientTag;
using AegiFinance.Application.Features.ClientTags.Commands.UpdateClientTag;
using AegiFinance.Application.Features.ClientTags.Queries.GetClientTags;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientTagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientTagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientTagDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientTagsQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ClientTagDto>> Create(CreateClientTagCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientTagDto>> Update(Guid id, UpdateClientTagCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClientTagCommand(id), cancellationToken);
        return NoContent();
    }
}
