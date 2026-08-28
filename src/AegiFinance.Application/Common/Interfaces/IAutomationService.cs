namespace AegiFinance.Application.Common.Interfaces;

public interface IAutomationService
{
    Task<AutomationCycleResult> ProduceAsync(CancellationToken cancellationToken = default);
    Task<OutboxDispatchResult> DispatchAsync(CancellationToken cancellationToken = default);
}

public sealed record AutomationCycleResult(int Renewed, int Expired, int RemindersQueued, int OverdueQueued, int ReconciliationQueued);
public sealed record OutboxDispatchResult(int Completed, int Retried, int DeadLettered);
