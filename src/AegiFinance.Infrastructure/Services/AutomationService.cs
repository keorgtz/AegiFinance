using System.Diagnostics;
using System.Text;
using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Automation;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AegiFinance.Infrastructure.Services;

public sealed class AutomationService : IAutomationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _clients;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AutomationService> _logger;
    private static readonly ActivitySource Activity = new("AegiFinance.Automation");

    public AutomationService(ApplicationDbContext context, IHttpClientFactory clients, IConfiguration configuration, ILogger<AutomationService> logger) =>
        (_context, _clients, _configuration, _logger) = (context, clients, configuration, logger);

    public async Task<AutomationCycleResult> ProduceAsync(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity("automation.produce");
        var now = DateTime.UtcNow;
        var today = now.Date;
        var renewed = 0; var expired = 0; var reminders = 0; var overdue = 0; var reconciliation = 0;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var ending = await _context.Subscriptions.IgnoreQueryFilters().Include(item => item.Client)
            .Where(item => !item.IsDeleted && item.Status == SubscriptionStatus.Active && item.EndDate.HasValue && item.EndDate.Value < today.AddDays(8))
            .OrderBy(item => item.EndDate).Take(500).ToListAsync(cancellationToken);

        foreach (var subscription in ending)
        {
            var endDate = subscription.EndDate!.Value.Date;
            if (endDate < today)
            {
                if (subscription.AutoRenew && subscription.BillingType is not BillingType.OneTime and not BillingType.Hourly)
                {
                    var key = $"subscription-renewed:{subscription.Id:N}:{endDate:yyyyMMdd}";
                    if (!await ExistsAsync(subscription.Client.OrganizationId, key, cancellationToken))
                    {
                        var nextEnd = AutomationRules.RenewalEnd(endDate, subscription.BillingType, subscription.CustomIntervalDays);
                        subscription.EndDate = nextEnd;
                        subscription.Renewals.Add(new SubscriptionRenewal
                        {
                            Id = Guid.NewGuid(), IdempotencyKey = key, PreviousEndDate = endDate, NewEndDate = nextEnd,
                            PreviousNextBillingDate = subscription.NextBillingDate, NewNextBillingDate = subscription.NextBillingDate,
                            RenewedAt = now, Reason = "Automatic renewal"
                        });
                        Queue(subscription.Client.OrganizationId, "subscription.renewed", key, new { subscriptionId = subscription.Id, previousEndDate = endDate, newEndDate = nextEnd }, now);
                        renewed++;
                    }
                }
                else
                {
                    var key = $"subscription-expired:{subscription.Id:N}:{endDate:yyyyMMdd}";
                    if (!await ExistsAsync(subscription.Client.OrganizationId, key, cancellationToken))
                    {
                        subscription.Status = SubscriptionStatus.Expired;
                        subscription.NextBillingDate = null;
                        Queue(subscription.Client.OrganizationId, "subscription.expired", key, new { subscriptionId = subscription.Id, endDate }, now);
                        expired++;
                    }
                }
            }
            else
            {
                var days = (endDate - today).Days;
                if (AutomationRules.IsReminderDay(days))
                {
                    var key = $"subscription-reminder:{subscription.Id:N}:{endDate:yyyyMMdd}:{days}";
                    if (!await ExistsAsync(subscription.Client.OrganizationId, key, cancellationToken))
                    {
                        Queue(subscription.Client.OrganizationId, "subscription.expiration-reminder", key, new { subscriptionId = subscription.Id, endDate, daysRemaining = days }, now);
                        reminders++;
                    }
                }
            }
        }

        var overdueItems = await _context.BillingItems.IgnoreQueryFilters().Include(item => item.Client)
            .Where(item => !item.IsDeleted && item.DueDate < today && item.Status != BillingItemStatus.Paid && item.Status != BillingItemStatus.Settled && item.Status != BillingItemStatus.Cancelled)
            .OrderBy(item => item.DueDate).Take(500).ToListAsync(cancellationToken);
        foreach (var item in overdueItems)
        {
            var daysOverdue = (today - item.DueDate.Date).Days;
            if (daysOverdue is not (1 or 3 or 7 or 15 or 30) && daysOverdue % 30 != 0) continue;
            var key = $"billing-overdue:{item.Id:N}:{daysOverdue}";
            if (await ExistsAsync(item.Client.OrganizationId, key, cancellationToken)) continue;
            Queue(item.Client.OrganizationId, "billing.overdue-reminder", key, new { billingItemId = item.Id, item.ClientId, item.DueDate, daysOverdue, outstanding = item.Amount - item.PaidAmount, item.Currency }, now);
            overdue++;
        }

        var accounts = await _context.BankAccounts.IgnoreQueryFilters().AsNoTracking().Where(item => !item.IsDeleted && item.IsActive).Take(500).ToListAsync(cancellationToken);
        foreach (var account in accounts)
        {
            var key = $"reconciliation-due:{account.Id:N}:{today:yyyyMMdd}";
            if (await ExistsAsync(account.OrganizationId, key, cancellationToken)) continue;
            Queue(account.OrganizationId, "reconciliation.review-requested", key, new { bankAccountId = account.Id, from = today.AddDays(-7), to = today }, now);
            reconciliation++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        activity?.SetTag("automation.queued", reminders + overdue + reconciliation + renewed + expired);
        return new AutomationCycleResult(renewed, expired, reminders, overdue, reconciliation);
    }

    public async Task<OutboxDispatchResult> DispatchAsync(CancellationToken cancellationToken = default)
    {
        using var activity = Activity.StartActivity("outbox.dispatch");
        var now = DateTime.UtcNow;
        var owner = $"{Environment.MachineName}:{Environment.ProcessId}";
        var batchSize = Math.Clamp(_configuration.GetValue("Automation:BatchSize", 50), 1, 200);
        var messages = await _context.OutboxMessages.IgnoreQueryFilters()
            .Where(item => !item.IsDeleted && (item.Status == OutboxMessageStatus.Pending || item.Status == OutboxMessageStatus.Failed || (item.Status == OutboxMessageStatus.Processing && item.LockedUntil < now)) && item.AvailableAt <= now)
            .OrderBy(item => item.AvailableAt).Take(batchSize).ToListAsync(cancellationToken);
        var completed = 0; var retried = 0; var dead = 0;
        foreach (var message in messages)
        {
            message.Status = OutboxMessageStatus.Processing;
            message.LockOwner = owner;
            message.LockedUntil = now.AddMinutes(5);
            message.AttemptCount++;
            message.TraceId ??= System.Diagnostics.Activity.Current?.TraceId.ToString();
            await _context.SaveChangesAsync(cancellationToken);
            try
            {
                await DeliverAsync(message, cancellationToken);
                message.Status = OutboxMessageStatus.Completed;
                message.ProcessedAt = DateTime.UtcNow;
                message.LockedUntil = null; message.LockOwner = null; message.LastError = null;
                completed++;
            }
            catch (Exception exception)
            {
                message.LastError = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
                message.LockedUntil = null; message.LockOwner = null;
                if (message.AttemptCount >= message.MaxAttempts) { message.Status = OutboxMessageStatus.DeadLetter; dead++; }
                else { message.Status = OutboxMessageStatus.Failed; message.AvailableAt = DateTime.UtcNow.Add(AutomationRules.RetryDelay(message.AttemptCount)); retried++; }
                _logger.LogWarning(exception, "Outbox message {MessageId} ({EventType}) failed on attempt {Attempt}.", message.Id, message.EventType, message.AttemptCount);
            }
            await _context.SaveChangesAsync(cancellationToken);
        }
        activity?.SetTag("outbox.completed", completed); activity?.SetTag("outbox.retried", retried); activity?.SetTag("outbox.dead_letter", dead);
        return new OutboxDispatchResult(completed, retried, dead);
    }

    private async Task DeliverAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        var endpoint = _configuration["Automation:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            _logger.LogInformation("Outbox event {EventType} completed without external delivery; id={MessageId}.", message.EventType, message.Id);
            return;
        }
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(new { message.Id, message.OrganizationId, message.EventType, message.IdempotencyKey, payload = JsonDocument.Parse(message.PayloadJson).RootElement }), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Idempotency-Key", message.IdempotencyKey);
        var secret = _configuration["Automation:WebhookSecret"];
        if (!string.IsNullOrWhiteSpace(secret)) request.Headers.Add("X-AegiFinance-Webhook-Secret", secret);
        using var response = await _clients.CreateClient("AegiFinance.Outbox").SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private Task<bool> ExistsAsync(Guid organizationId, string key, CancellationToken cancellationToken) =>
        _context.OutboxMessages.IgnoreQueryFilters().AnyAsync(item => item.OrganizationId == organizationId && item.IdempotencyKey == key, cancellationToken);

    private void Queue(Guid organizationId, string type, string key, object payload, DateTime now) => _context.OutboxMessages.Add(new OutboxMessage
    {
        Id = Guid.NewGuid(), OrganizationId = organizationId, EventType = type, IdempotencyKey = key,
        PayloadJson = JsonSerializer.Serialize(payload), Status = OutboxMessageStatus.Pending, AvailableAt = now,
        MaxAttempts = Math.Clamp(_configuration.GetValue("Automation:MaxAttempts", 5), 1, 20)
    });
}
