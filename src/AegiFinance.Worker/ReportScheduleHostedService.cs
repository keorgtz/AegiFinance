using AegiFinance.Application.Common.Interfaces;

namespace AegiFinance.Worker;

public sealed class ReportScheduleHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportScheduleHostedService> _logger;
    public ReportScheduleHostedService(IServiceProvider services, IConfiguration configuration, ILogger<ReportScheduleHostedService> logger) =>
        (_services, _configuration, _logger) = (services, configuration, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var seconds = Math.Clamp(_configuration.GetValue("Reporting:PollIntervalSeconds", 60), 15, 3600);
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(seconds));
        do
        {
            try
            {
                using var scope = _services.CreateScope();
                var processed = await scope.ServiceProvider.GetRequiredService<IReportScheduleProcessor>().ProcessDueAsync(stoppingToken);
                if (processed > 0) _logger.LogInformation("Generated {Count} scheduled financial reports.", processed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { _logger.LogError(exception, "Scheduled financial report processing failed."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
