using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.ExchangeRates.Commands.CreateExchangeRate;
using AegiFinance.Application.Features.ExchangeRates.Commands.SyncExchangeRate;
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

    [HttpPost]
    public async Task<ActionResult<ExchangeRateDto>> Create(CreateExchangeRateCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpPost("sync")]
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
