using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.ClientCategories.Commands.CreateClientCategory;
using AegiFinance.Application.Features.ClientCategories.Commands.DeleteClientCategory;
using AegiFinance.Application.Features.ClientCategories.Commands.UpdateClientCategory;
using AegiFinance.Application.Features.ClientCategories.Queries.GetClientCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ClientCategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetClientCategoriesQuery(), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ClientCategoryDto>> Create(CreateClientCategoryCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClientCategoryDto>> Update(Guid id, UpdateClientCategoryCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteClientCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
