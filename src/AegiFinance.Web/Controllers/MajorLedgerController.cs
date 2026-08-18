using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/major-ledger")]
[Authorize]
public sealed class MajorLedgerController : ControllerBase
{
    private readonly IMajorLedgerService _majorLedger;

    public MajorLedgerController(IMajorLedgerService majorLedger) => _majorLedger = majorLedger;

    [HttpGet("accounts")]
    [Authorize(Policy = "ViewMajorLedger")]
    public async Task<ActionResult<IReadOnlyList<GeneralLedgerAccountDto>>> GetAccounts([FromQuery] DateTime? asOfDate, CancellationToken cancellationToken)
        => Ok(await _majorLedger.GetAccountsAsync(asOfDate, cancellationToken));

    [HttpGet("periods")]
    [Authorize(Policy = "ViewMajorLedger")]
    public async Task<ActionResult<IReadOnlyList<AccountingPeriodDto>>> GetPeriods(CancellationToken cancellationToken)
        => Ok(await _majorLedger.GetPeriodsAsync(cancellationToken));

    [HttpPost("periods")]
    [Authorize(Policy = "ManageAccountingPeriods")]
    public async Task<ActionResult<AccountingPeriodDto>> CreatePeriod(CreateAccountingPeriodRequest request, CancellationToken cancellationToken)
        => Ok(await _majorLedger.CreatePeriodAsync(request.Name, request.StartDate, request.EndDate, cancellationToken));

    [HttpPost("periods/{id:guid}/close")]
    [Authorize(Policy = "ManageAccountingPeriods")]
    public async Task<ActionResult<AccountingPeriodDto>> ClosePeriod(Guid id, CancellationToken cancellationToken)
        => Ok(await _majorLedger.ClosePeriodAsync(id, cancellationToken));

    [HttpGet("journal-entries")]
    [Authorize(Policy = "ViewMajorLedger")]
    public async Task<ActionResult<IReadOnlyList<JournalEntryDto>>> GetEntries(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status, CancellationToken cancellationToken)
        => Ok(await _majorLedger.GetJournalEntriesAsync(from, to, status, cancellationToken));

    [HttpPost("journal-entries")]
    [Authorize(Policy = "CreateJournalEntries")]
    public async Task<ActionResult<JournalEntryDto>> CreateDraft(CreateJournalEntryRequest request, CancellationToken cancellationToken)
        => Ok(await _majorLedger.CreateDraftAsync(request, cancellationToken));

    [HttpPost("journal-entries/{id:guid}/post")]
    [Authorize(Policy = "PostJournalEntries")]
    public async Task<ActionResult<JournalEntryDto>> Post(Guid id, CancellationToken cancellationToken)
        => Ok(await _majorLedger.PostAsync(id, cancellationToken));

    [HttpPost("journal-entries/{id:guid}/reverse")]
    [Authorize(Policy = "ReverseJournalEntries")]
    public async Task<ActionResult<JournalEntryDto>> Reverse(Guid id, ReverseJournalEntryRequest request, CancellationToken cancellationToken)
        => Ok(await _majorLedger.ReverseAsync(id, request.Date, request.Reason, cancellationToken));

    [HttpGet("trial-balance")]
    [Authorize(Policy = "ViewMajorLedger")]
    public async Task<ActionResult<TrialBalanceDto>> GetTrialBalance(
        [FromQuery] DateTime? asOfDate, [FromQuery] string currency = "MXN", CancellationToken cancellationToken = default)
        => Ok(await _majorLedger.GetTrialBalanceAsync(asOfDate ?? DateTime.UtcNow, currency.ToUpperInvariant(), cancellationToken));

    [HttpGet("accounts/{id:guid}/ledger")]
    [Authorize(Policy = "ViewMajorLedger")]
    public async Task<ActionResult<IReadOnlyList<AccountLedgerLineDto>>> GetAccountLedger(
        Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
        => Ok(await _majorLedger.GetAccountLedgerAsync(id, from, to, cancellationToken));

    [HttpPost("legacy-migration")]
    [Authorize(Policy = "MigrateMajorLedger")]
    public async Task<ActionResult<LegacyMigrationResultDto>> MigrateLegacy(CancellationToken cancellationToken)
        => Ok(await _majorLedger.MigrateLegacyAsync(cancellationToken));
}

public record CreateAccountingPeriodRequest(string Name, DateTime StartDate, DateTime EndDate);
public record ReverseJournalEntryRequest(DateTime Date, string Reason);
