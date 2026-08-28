using AegiFinance.Application.Common.Interfaces;

namespace AegiFinance.Worker;

public sealed class AutomationHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutomationHostedService> _logger;

    public AutomationHostedService(IServiceProvider services, IConfiguration configuration, ILogger<AutomationHostedService> logger) =>
        (_services, _configuration, _logger) = (services, configuration, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollSeconds = Math.Clamp(_configuration.GetValue("Automation:PollIntervalSeconds", 30), 10, 3600);
        var productionMinutes = Math.Clamp(_configuration.GetValue("Automation:ProductionIntervalMinutes", 15), 1, 1440);
        var nextProduction = DateTime.MinValue;
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(pollSeconds));
        do
        {
            try
            {
                using var scope = _services.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAutomationService>();
                if (DateTime.UtcNow >= nextProduction)
                {
                    var produced = await service.ProduceAsync(stoppingToken);
                    nextProduction = DateTime.UtcNow.AddMinutes(productionMinutes);
                    _logger.LogInformation("Automation production completed: {Result}.", produced);
                }
                var dispatched = await service.DispatchAsync(stoppingToken);
                if (dispatched.Completed + dispatched.Retried + dispatched.DeadLettered > 0)
                    _logger.LogInformation("Outbox dispatch completed: {Result}.", dispatched);
                await File.WriteAllTextAsync("/tmp/aegifinance-worker-heartbeat", DateTime.UtcNow.ToString("O"), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception exception) { _logger.LogError(exception, "Automation/outbox processing cycle failed."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
