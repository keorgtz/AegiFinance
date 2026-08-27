using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/accounting-governance")]
[Authorize]
public sealed class AccountingGovernanceController : ControllerBase
{
    private readonly IAccountingGovernanceService _service;
    public AccountingGovernanceController(IAccountingGovernanceService service) => _service = service;

    [HttpGet("periods")]
    [Authorize(Policy = "ViewAccountingIntegrity")]
    public async Task<ActionResult<IReadOnlyList<AccountingPeriodDto>>> Periods(CancellationToken cancellationToken) =>
        Ok(await _service.GetPeriodsAsync(cancellationToken));

    [HttpGet("periods/{id:guid}/checklist")]
    [Authorize(Policy = "ViewAccountingIntegrity")]
    public async Task<ActionResult<AccountingPeriodChecklistDto>> Checklist(Guid id, CancellationToken cancellationToken) =>
        Ok(await _service.GetChecklistAsync(id, cancellationToken));

    [HttpPost("periods/{id:guid}/close")]
    [Authorize(Policy = "CloseAccountingPeriods")]
    public async Task<ActionResult<AccountingPeriodDto>> Close(Guid id, CloseAccountingPeriodRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.CloseAsync(id, request.VerificationCode, cancellationToken));

    [HttpPost("periods/{id:guid}/reopen-requests")]
    [Authorize(Policy = "RequestAccountingPeriodReopen")]
    public async Task<ActionResult<AccountingPeriodReopenRequestDto>> RequestReopen(Guid id, RequestAccountingPeriodReopenRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.RequestReopenAsync(id, request.Reason, cancellationToken));

    [HttpGet("reopen-requests")]
    [Authorize(Policy = "ViewAccountingAudit")]
    public async Task<ActionResult<IReadOnlyList<AccountingPeriodReopenRequestDto>>> ReopenRequests(CancellationToken cancellationToken) =>
        Ok(await _service.GetReopenRequestsAsync(cancellationToken));

    [HttpPost("reopen-requests/{id:guid}/review")]
    [Authorize(Policy = "ApproveAccountingPeriodReopen")]
    public async Task<ActionResult<AccountingPeriodReopenRequestDto>> ReviewReopen(Guid id, ReviewAccountingPeriodReopenRequest request, CancellationToken cancellationToken) =>
        Ok(await _service.ReviewReopenAsync(id, request.Approve, request.Comment, cancellationToken));

    [HttpGet("integrity-alerts")]
    [Authorize(Policy = "ViewAccountingIntegrity")]
    public async Task<ActionResult<IReadOnlyList<AccountingIntegrityAlertDto>>> IntegrityAlerts(CancellationToken cancellationToken) =>
        Ok(await _service.GetIntegrityAlertsAsync(cancellationToken));

    [HttpGet("evidence")]
    [Authorize(Policy = "ViewAccountingAudit")]
    public async Task<ActionResult<AccountingEvidenceSummaryDto>> Evidence([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken) =>
        Ok(await _service.GetEvidenceAsync(from, to, cancellationToken));
}
