using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = "ViewDashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    public DashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary([FromQuery] GetDashboardSummaryQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("attention")]
    public async Task<ActionResult<DashboardAttentionDto>> Attention([FromQuery] GetDashboardAttentionQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("activity")]
    public async Task<ActionResult<DashboardActivityDto>> Activity([FromQuery] GetDashboardActivityQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("import-attempts")]
    [Authorize(Policy = "ManageReconciliation")]
    public async Task<ActionResult<List<BankImportAttemptDto>>> ImportAttempts([FromQuery] GetBankImportAttemptsQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("unreconciled-lines")]
    [Authorize(Policy = "ManageReconciliation")]
    public async Task<ActionResult<List<UnreconciledBankLineDto>>> UnreconciledLines([FromQuery] GetUnreconciledBankLinesQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));
}
