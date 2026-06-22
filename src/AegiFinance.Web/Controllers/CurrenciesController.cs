using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Currencies.Commands.CreateCurrencyConfig;
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
    public async Task<ActionResult<CurrencyConfigDto>> Create(CreateCurrencyConfigCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
