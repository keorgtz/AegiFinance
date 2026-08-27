using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientFinancialSummary;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientMovements;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountStatementsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAccountStatementExporter _exporter;

    public AccountStatementsController(IMediator mediator, IAccountStatementExporter exporter)
    {
        _mediator = mediator;
        _exporter = exporter;
    }

    [HttpGet("clients")]
    [Authorize(Policy = "ViewAccountStatements")]
    public async Task<ActionResult<IReadOnlyList<AccountStatementClientOptionDto>>> Clients(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetAccountStatementClientsQuery(), cancellationToken));

    [HttpGet("client/{clientId:guid}/subscriptions")]
    [Authorize(Policy = "ViewAccountStatements")]
    public async Task<ActionResult<IReadOnlyList<AccountStatementSubscriptionOptionDto>>> Subscriptions(Guid clientId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetAccountStatementSubscriptionsQuery(clientId), cancellationToken));

    [HttpGet("client/{clientId:guid}")]
    [Authorize(Policy = "ViewAccountStatements")]
    public async Task<ActionResult<AccountStatementDto>> GetStatement(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", [FromQuery] Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientStatementQuery
        {
            ClientId = clientId,
            From = from,
            To = to,
            Currency = currency,
            SubscriptionId = subscriptionId
        }, cancellationToken));
    }

    [HttpGet("client/{clientId:guid}/summary")]
    [Authorize(Policy = "ViewAccountStatements")]
    public async Task<ActionResult<FinancialSummaryDto>> GetSummary(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", [FromQuery] Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientFinancialSummaryQuery
        {
            ClientId = clientId,
            From = from,
            To = to,
            Currency = currency,
            SubscriptionId = subscriptionId
        }, cancellationToken));
    }

    [HttpGet("client/{clientId:guid}/movements")]
    [Authorize(Policy = "ViewAccountStatements")]
    public async Task<ActionResult<List<AccountStatementItemDto>>> GetMovements(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", [FromQuery] Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        return Ok(await _mediator.Send(new GetClientMovementsQuery
        {
            ClientId = clientId,
            From = from,
            To = to,
            Currency = currency,
            SubscriptionId = subscriptionId
        }, cancellationToken));
    }

    [HttpGet("client/{clientId:guid}/export.pdf")]
    [Authorize(Policy = "ExportAccountStatements")]
    public async Task<IActionResult> ExportPdf(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", [FromQuery] Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        var statement = await Statement(clientId, from, to, currency, subscriptionId, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-AegiFinance-Verification"] = statement.VerificationCode;
        return File(_exporter.CreatePdf(statement), "application/pdf", FileName(statement, "pdf"));
    }

    [HttpGet("client/{clientId:guid}/export.csv")]
    [Authorize(Policy = "ExportAccountStatements")]
    public async Task<IActionResult> ExportCsv(Guid clientId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string currency = "MXN", [FromQuery] Guid? subscriptionId = null, CancellationToken cancellationToken = default)
    {
        var statement = await Statement(clientId, from, to, currency, subscriptionId, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-AegiFinance-Verification"] = statement.VerificationCode;
        return File(_exporter.CreateCsv(statement), "text/csv; charset=utf-8", FileName(statement, "csv"));
    }

    [HttpGet("client/{clientId:guid}/inquiries")]
    [Authorize(Policy = "ViewAccountStatementInquiries")]
    public async Task<ActionResult<IReadOnlyList<AccountStatementInquiryDto>>> Inquiries(Guid clientId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetAccountStatementInquiriesQuery(clientId), cancellationToken));

    [HttpPost("client/{clientId:guid}/inquiries")]
    [Authorize(Policy = "CreateAccountStatementInquiries")]
    public async Task<ActionResult<AccountStatementInquiryDto>> CreateInquiry(Guid clientId, CreateAccountStatementInquiryCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = clientId;
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Inquiries), new { clientId }, result);
    }

    [HttpPut("inquiries/{id:guid}/resolve")]
    [Authorize(Policy = "ResolveAccountStatementInquiries")]
    public async Task<ActionResult<AccountStatementInquiryDto>> ResolveInquiry(Guid id, AccountStatementResolutionRequest request, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ResolveAccountStatementInquiryCommand(id, request.Resolution), cancellationToken));

    private Task<AccountStatementDto> Statement(Guid clientId, DateTime? from, DateTime? to, string currency, Guid? subscriptionId, CancellationToken cancellationToken) =>
        _mediator.Send(new GetClientStatementQuery { ClientId = clientId, From = from, To = to, Currency = currency, SubscriptionId = subscriptionId }, cancellationToken);

    private static string FileName(AccountStatementDto statement, string extension) =>
        $"estado-cuenta-{statement.ClientId:N}-{statement.StatementDate:yyyyMMdd}.{extension}";
}

public sealed class AccountStatementResolutionRequest { public string Resolution { get; set; } = string.Empty; }
