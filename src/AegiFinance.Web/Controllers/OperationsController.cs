using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/operations")]
[Authorize(Policy = "ViewOperations")]
public sealed class OperationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    public OperationsController(ApplicationDbContext context) => _context = context;

    [HttpGet("outbox")]
    public async Task<IActionResult> GetOutbox(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var summary = await _context.OutboxMessages.AsNoTracking().GroupBy(item => item.Status)
            .Select(group => new { status = group.Key.ToString(), count = group.Count(), oldestAvailableAt = group.Min(item => item.AvailableAt) })
            .ToListAsync(cancellationToken);
        return Ok(new { utc = now, healthy = !summary.Any(item => item.status == nameof(OutboxMessageStatus.DeadLetter) && item.count > 0), summary });
    }

    [HttpGet("metrics")]
    [Produces("text/plain")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var counts = await _context.OutboxMessages.AsNoTracking().GroupBy(item => item.Status)
            .Select(group => new { group.Key, Count = group.Count() }).ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);
        var lines = Enum.GetValues<OutboxMessageStatus>().Select(status => $"aegifinance_outbox_messages{{status=\"{status.ToString().ToLowerInvariant()}\"}} {counts.GetValueOrDefault(status)}").ToList();
        lines.Add($"aegifinance_process_uptime_seconds {Environment.TickCount64 / 1000}");
        return Content(string.Join('\n', lines) + "\n", "text/plain");
    }
}
