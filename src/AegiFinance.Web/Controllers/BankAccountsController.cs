using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.BankAccounts.Commands.CreateBankAccount;
using AegiFinance.Application.Features.BankAccounts.Commands.DeleteBankAccount;
using AegiFinance.Application.Features.BankAccounts.Commands.UpdateBankAccount;
using AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountBalance;
using AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccountById;
using AegiFinance.Application.Features.BankAccounts.Queries.GetBankAccounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BankAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<BankAccountListDto>>> GetAll([FromQuery] GetBankAccountsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BankAccountDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBankAccountByIdQuery(id), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<BankAccountDto>> Create(CreateBankAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BankAccountDto>> Update(Guid id, UpdateBankAccountCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteBankAccountCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/balance")]
    public async Task<ActionResult<decimal>> GetBalance(Guid id, [FromQuery] DateTime? asOfDate, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(new GetBankAccountBalanceQuery(id, asOfDate), cancellationToken));
    }
}
