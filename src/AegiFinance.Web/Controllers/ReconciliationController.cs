using AegiFinance.Application.Features.Reconciliation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/reconciliation")]
[Authorize]
public sealed class ReconciliationController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReconciliationController(IMediator mediator) => _mediator = mediator;

    [HttpGet("cases")]
    [Authorize(Policy = "ViewReconciliation")]
    public async Task<ActionResult<IReadOnlyList<ReconciliationCaseDto>>> Cases([FromQuery] GetReconciliationCasesQuery query, CancellationToken cancellationToken) => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("cases/{id:guid}")]
    [Authorize(Policy = "ViewReconciliation")]
    public async Task<ActionResult<ReconciliationCaseDto>> Case(Guid id, CancellationToken cancellationToken) => Ok(await _mediator.Send(new GetReconciliationCaseQuery(id), cancellationToken));

    [HttpPost("run")]
    [Authorize(Policy = "RunReconciliation")]
    public async Task<ActionResult<ReconciliationRunResultDto>> Run(RunReconciliationCommand command, CancellationToken cancellationToken) => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPost("cases/manual")]
    [Authorize(Policy = "ConfirmReconciliation")]
    public async Task<ActionResult<ReconciliationCaseDto>> Manual(CreateManualReconciliationCommand command, CancellationToken cancellationToken) => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPost("cases/{id:guid}/confirm")]
    [Authorize(Policy = "ConfirmReconciliation")]
    public async Task<ActionResult<ReconciliationCaseDto>> Confirm(Guid id, ConfirmReconciliationCaseCommand command, CancellationToken cancellationToken)
    { command.Id = id; return Ok(await _mediator.Send(command, cancellationToken)); }

    [HttpPost("cases/{id:guid}/reject")]
    [Authorize(Policy = "ConfirmReconciliation")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ReconciliationReasonRequest request, CancellationToken cancellationToken)
    { await _mediator.Send(new RejectReconciliationCaseCommand(id, request.Reason), cancellationToken); return NoContent(); }

    [HttpPost("cases/{id:guid}/reverse")]
    [Authorize(Policy = "ReverseReconciliation")]
    public async Task<IActionResult> Reverse(Guid id, [FromBody] ReconciliationReasonRequest request, CancellationToken cancellationToken)
    { await _mediator.Send(new ReverseReconciliationCaseCommand(id, request.Reason ?? string.Empty), cancellationToken); return NoContent(); }

    [HttpGet("settings")]
    [Authorize(Policy = "ViewReconciliation")]
    public async Task<ActionResult<ReconciliationSettingsDto>> Settings(CancellationToken cancellationToken) => Ok(await _mediator.Send(new GetReconciliationSettingsQuery(), cancellationToken));

    [HttpPut("settings")]
    [Authorize(Policy = "ManageReconciliationSettings")]
    public async Task<ActionResult<ReconciliationSettingsDto>> UpdateSettings(UpdateReconciliationSettingsCommand command, CancellationToken cancellationToken) => Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet("periods")]
    [Authorize(Policy = "ViewReconciliation")]
    public async Task<ActionResult<IReadOnlyList<ReconciliationPeriodDto>>> Periods([FromQuery] GetReconciliationPeriodsQuery query, CancellationToken cancellationToken) => Ok(await _mediator.Send(query, cancellationToken));

    [HttpPost("periods/close")]
    [Authorize(Policy = "CloseReconciliationPeriods")]
    public async Task<ActionResult<ReconciliationPeriodDto>> ClosePeriod(CloseReconciliationPeriodCommand command, CancellationToken cancellationToken) => Ok(await _mediator.Send(command, cancellationToken));
}

public sealed class ReconciliationReasonRequest { public string? Reason { get; set; } }
