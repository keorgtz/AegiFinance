using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Services.Commands.ActivateService;
using AegiFinance.Application.Features.Services.Commands.AddServicePriceHistory;
using AegiFinance.Application.Features.Services.Commands.CreateService;
using AegiFinance.Application.Features.Services.Commands.DeactivateService;
using AegiFinance.Application.Features.Services.Commands.DeleteService;
using AegiFinance.Application.Features.Services.Commands.UpdateService;
using AegiFinance.Application.Features.Services.Commands.CreateServiceVersion;
using AegiFinance.Application.Features.Services.Queries.GetServiceById;
using AegiFinance.Application.Features.Services.Queries.GetServicePriceHistory;
using AegiFinance.Application.Features.Services.Queries.GetServices;
using AegiFinance.Application.Features.Services.Queries.GetServiceVersions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServicesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ViewServices")]
    public async Task<ActionResult<PaginatedList<ServiceListDto>>> GetAll([FromQuery] GetServicesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ViewServices")]
    public async Task<ActionResult<ServiceDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetServiceByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "CreateServices")]
    public async Task<ActionResult<ServiceDto>> Create(CreateServiceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "UpdateServices")]
    public async Task<ActionResult<ServiceDto>> Update(Guid id, UpdateServiceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "DeleteServices")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteServiceCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "UpdateServices")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateServiceCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "UpdateServices")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateServiceCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/price-history")]
    [Authorize(Policy = "ViewServices")]
    public async Task<ActionResult<List<ServicePriceHistoryDto>>> GetPriceHistory(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetServicePriceHistoryQuery(id), cancellationToken));
    }

    [HttpPost("{id:guid}/price-history")]
    [Authorize(Policy = "UpdateServices")]
    public async Task<ActionResult<ServicePriceHistoryDto>> AddPriceHistory(Guid id, AddServicePriceHistoryCommand command, CancellationToken cancellationToken)
    {
        command.ServiceId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpGet("{id:guid}/versions")]
    [Authorize(Policy = "ViewServices")]
    public async Task<ActionResult<List<ServiceVersionDto>>> GetVersions(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetServiceVersionsQuery(id), cancellationToken));

    [HttpPost("{id:guid}/versions")]
    [Authorize(Policy = "CreateServiceVersions")]
    public async Task<ActionResult<ServiceVersionDto>> CreateVersion(Guid id, CreateServiceVersionCommand command, CancellationToken cancellationToken)
    {
        command.ServiceId = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}
