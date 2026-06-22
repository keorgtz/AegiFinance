using AegiFinance.Application.Common.Interfaces;

namespace AegiFinance.Worker;

public class BillingGenerationHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BillingGenerationHostedService> _logger;

    public BillingGenerationHostedService(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<BillingGenerationHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var scheduledTime = GetScheduledTime();
            var nextRun = GetNextRunTime(now, scheduledTime);
            var delay = nextRun - now;

            _logger.LogInformation(
                "Próxima generación de cargos programada para: {nextRun} (en {delay})",
                nextRun,
                delay);

            await Task.Delay(delay, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await RunBillingGenerationAsync(stoppingToken);
        }
    }

    private async Task RunBillingGenerationAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var billingService = scope.ServiceProvider.GetRequiredService<IBillingGenerationService>();

            var now = DateTime.UtcNow;
            _logger.LogInformation("Iniciando generación de cargos para {year}/{month}", now.Year, now.Month);

            var result = await billingService.GenerateForCycleAsync(now.Year, now.Month, null, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Generación de cargos completada. Items generados: {count}", result.ItemsGenerated);
            }
            else
            {
                _logger.LogWarning(
                    "Generación de cargos finalizada con errores. Items generados: {count}. Errores: {errors}",
                    result.ItemsGenerated,
                    string.Join("; ", result.Errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ejecutar la generación de cargos.");
        }
    }

    private TimeSpan GetScheduledTime()
    {
        var value = _configuration["Billing:DailyRunTime"] ?? "02:00";
        if (TimeSpan.TryParse(value, out var time))
        {
            return time;
        }

        _logger.LogWarning("Formato inválido para Billing:DailyRunTime '{value}'. Usando 02:00.", value);
        return TimeSpan.FromHours(2);
    }

    private static DateTime GetNextRunTime(DateTime now, TimeSpan scheduledTime)
    {
        var nextRun = now.Date.Add(scheduledTime);
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }

        return nextRun;
    }
}
