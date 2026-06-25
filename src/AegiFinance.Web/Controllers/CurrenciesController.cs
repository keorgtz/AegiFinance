using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Currencies.Commands.CreateCurrencyConfig;
using AegiFinance.Application.Features.Currencies.Commands.DeleteCurrencyConfig;
using AegiFinance.Application.Features.Currencies.Commands.UpdateCurrencyConfig;
using AegiFinance.Application.Features.Currencies.Queries.GetCurrencies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CurrenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CurrenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CurrencyConfigDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetCurrenciesQuery(), cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "ManageBilling")]
    public async Task<ActionResult<CurrencyConfigDto>> Create(CreateCurrencyConfigCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageBilling")]
    public async Task<ActionResult<CurrencyConfigDto>> Update(Guid id, UpdateCurrencyConfigCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageBilling")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCurrencyConfigCommand(id), cancellationToken);
        return NoContent();
    }
}
