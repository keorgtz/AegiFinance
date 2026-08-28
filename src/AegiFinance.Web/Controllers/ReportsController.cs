using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "ViewReports")]
public sealed class ReportsController : ControllerBase
{
    private readonly IFinancialReportingService _reports;
    public ReportsController(IFinancialReportingService reports) => _reports = reports;

    [HttpGet]
    public async Task<ActionResult<FinancialReportDto>> Get([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken) =>
        Ok(await _reports.GenerateAsync(query, cancellationToken));

    [HttpGet("accounts")]
    [Authorize(Policy = "ViewAccountingReports")]
    public async Task<ActionResult<IReadOnlyList<ReportAccountDto>>> Accounts(CancellationToken cancellationToken) => Ok(await _reports.GetAccountsAsync(cancellationToken));

    [HttpGet("export")]
    [Authorize(Policy = "ExportReports")]
    public async Task<IActionResult> Export([FromQuery] FinancialReportQueryDto query, CancellationToken cancellationToken)
    {
        var report = await _reports.GenerateAsync(query, cancellationToken);
        return File(_reports.CreateCsv(report), "text/csv; charset=utf-8", $"{report.Kind.ToLowerInvariant()}-{report.To:yyyyMMdd}.csv");
    }

    [HttpGet("schedules")]
    [Authorize(Policy = "ManageReportSchedules")]
    public async Task<ActionResult<IReadOnlyList<ReportScheduleDto>>> Schedules(CancellationToken cancellationToken) => Ok(await _reports.GetSchedulesAsync(cancellationToken));

    [HttpPost("schedules")]
    [Authorize(Policy = "ManageReportSchedules")]
    public async Task<ActionResult<ReportScheduleDto>> CreateSchedule(SaveReportScheduleRequest request, CancellationToken cancellationToken) => Ok(await _reports.SaveScheduleAsync(null, request, cancellationToken));

    [HttpPut("schedules/{id:guid}")]
    [Authorize(Policy = "ManageReportSchedules")]
    public async Task<ActionResult<ReportScheduleDto>> UpdateSchedule(Guid id, SaveReportScheduleRequest request, CancellationToken cancellationToken) => Ok(await _reports.SaveScheduleAsync(id, request, cancellationToken));

    [HttpDelete("schedules/{id:guid}")]
    [Authorize(Policy = "ManageReportSchedules")]
    public async Task<IActionResult> DeleteSchedule(Guid id, CancellationToken cancellationToken)
    { await _reports.DeleteScheduleAsync(id, cancellationToken); return NoContent(); }

    [HttpGet("runs")]
    [Authorize(Policy = "ViewScheduledReportRuns")]
    public async Task<ActionResult<IReadOnlyList<ReportRunDto>>> Runs(CancellationToken cancellationToken) => Ok(await _reports.GetRunsAsync(cancellationToken));

    [HttpGet("runs/{id:guid}/file")]
    [Authorize(Policy = "DownloadScheduledReports")]
    public async Task<IActionResult> RunFile(Guid id, CancellationToken cancellationToken)
    { var file = await _reports.GetRunFileAsync(id, cancellationToken); return File(file.Content, file.ContentType, file.FileName); }
}
