using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using AegiFinance.Application.Features.ExchangeRates.Commands.DeleteExchangeRate;
using AegiFinance.Application.Features.ExchangeRates.Commands.SyncExchangeRate;
using AegiFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;
using AegiFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExchangeRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ExchangeRateDto>>> GetAll([FromQuery] GetExchangeRatesQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpPost]
    [Authorize(Policy = "ManageExchangeRates")]
    public async Task<ActionResult<ExchangeRateDto>> Create(CreateExchangeRateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ManageExchangeRates")]
    public async Task<ActionResult<ExchangeRateDto>> Update(Guid id, UpdateExchangeRateCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ManageExchangeRates")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteExchangeRateCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("sync")]
    [Authorize(Policy = "SyncExchangeRates")]
    public async Task<ActionResult<ExchangeRateDto>> Sync(SyncExchangeRateCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result is null)
        {
            return BadRequest("No se pudo sincronizar el tipo de cambio.");
        }

        return Ok(result);
    }
}
