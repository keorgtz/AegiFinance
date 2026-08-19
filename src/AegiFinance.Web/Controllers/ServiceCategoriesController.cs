using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.ServiceCategories.Commands.CreateServiceCategory;
using AegiFinance.Application.Features.ServiceCategories.Commands.DeleteServiceCategory;
using AegiFinance.Application.Features.ServiceCategories.Commands.UpdateServiceCategory;
using AegiFinance.Application.Features.ServiceCategories.Queries.GetServiceCategories;
using AegiFinance.Application.Features.ServiceCategories.Queries.GetServiceCategoryById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/service-categories")]
[Authorize]
public class ServiceCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServiceCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServiceCategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetServiceCategoriesQuery(), cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceCategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetServiceCategoryByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "ManageServices")]
    public async Task<ActionResult<ServiceCategoryDto>> Create(CreateServiceCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageServices")]
    public async Task<ActionResult<ServiceCategoryDto>> Update(Guid id, UpdateServiceCategoryCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageServices")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteServiceCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}
