using AegiFinance.Application.Common.Helpers;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class BillingGenerationService : IBillingGenerationService
{
    private readonly ApplicationDbContext _context;
    private readonly IMajorLedgerService _majorLedger;

    public BillingGenerationService(ApplicationDbContext context, IMajorLedgerService majorLedger)
    {
        _context = context;
        _majorLedger = majorLedger;
    }

    public async Task<BillingGenerationResult> GenerateForCycleAsync(int year, int? month, Guid? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var log = await StartLogAsync(null, triggeredBy, cancellationToken);
        var result = new BillingGenerationResult();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var (startDate, endDate) = CalculateCycleDates(year, month);
            var cycle = await FindOrCreateCycleAsync(year, month, startDate, endDate, cancellationToken);

            log.BillingCycleId = cycle.Id;

            var subscriptions = await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Service)
                .Include(s => s.TermsVersions)
                .Where(s => s.Status == SubscriptionStatus.Active
                    && s.NextBillingDate.HasValue
                    && s.NextBillingDate.Value >= startDate
                    && s.NextBillingDate.Value <= endDate)
                .ToListAsync(cancellationToken);

            foreach (var subscription in subscriptions)
            {
                try
                {
                    await GenerateItemForSubscriptionAsync(subscription, cycle, triggeredBy, cancellationToken);
                    result.ItemsGenerated++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Subscription {subscription.Code}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result.Success = result.Errors.Count == 0;
            await FinishLogAsync(log, result, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            result.Success = false;
            result.Errors.Add($"Error general: {ex.Message}");
            await FinishLogAsync(log, result, cancellationToken);
            return result;
        }
    }

    public async Task<BillingGenerationResult> GenerateForSubscriptionAsync(Guid subscriptionId, Guid? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var result = new BillingGenerationResult();

        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Service)
            .Include(s => s.TermsVersions)
            .FirstOrDefaultAsync(s => s.Id == subscriptionId, cancellationToken);

        if (subscription is null)
        {
            result.Errors.Add("La suscripción no existe.");
            return result;
        }

        if (subscription.Status != SubscriptionStatus.Active)
        {
            result.Errors.Add("La suscripción no está activa.");
            return result;
        }

        if (!subscription.NextBillingDate.HasValue)
        {
            result.Errors.Add("La suscripción no tiene fecha de facturación configurada.");
            return result;
        }

        var nextBilling = subscription.NextBillingDate.Value;
        var log = await StartLogAsync(null, triggeredBy, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var (startDate, endDate) = CalculateCycleDates(nextBilling.Year, nextBilling.Month);
            var cycle = await FindOrCreateCycleAsync(nextBilling.Year, nextBilling.Month, startDate, endDate, cancellationToken);

            log.BillingCycleId = cycle.Id;

            await GenerateItemForSubscriptionAsync(subscription, cycle, triggeredBy, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result.ItemsGenerated = 1;
            result.Success = true;
            await FinishLogAsync(log, result, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            result.Success = false;
            result.Errors.Add($"Error: {ex.Message}");
            await FinishLogAsync(log, result, cancellationToken);
            return result;
        }
    }

    public async Task<BillingGenerationResult> ReprocessCycleAsync(Guid billingCycleId, bool onlyPending = true, Guid? triggeredBy = null, CancellationToken cancellationToken = default)
    {
        var result = new BillingGenerationResult();

        var cycle = await _context.BillingCycles
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == billingCycleId, cancellationToken);

        if (cycle is null)
        {
            result.Errors.Add("El ciclo de facturación no existe.");
            return result;
        }

        var originalCycleStatus = cycle.Status;
        cycle.Status = BillingCycleStatus.Reprocessing;
        _context.BillingCycles.Update(cycle);

        var log = await StartLogAsync(cycle.Id, triggeredBy, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingItems = await _context.BillingItems
                .Where(bi => bi.BillingCycleId == billingCycleId)
                .ToListAsync(cancellationToken);

            var subscriptions = await _context.Subscriptions
                .AsNoTracking()
                .Include(s => s.Service)
                .Include(s => s.TermsVersions)
                .Where(s => s.Status == SubscriptionStatus.Active
                    && s.NextBillingDate.HasValue
                    && s.NextBillingDate.Value >= cycle.StartDate
                    && s.NextBillingDate.Value <= cycle.EndDate)
                .ToListAsync(cancellationToken);

            foreach (var subscription in subscriptions)
            {
                try
                {
                    var existingItem = existingItems.FirstOrDefault(bi => bi.IdempotencyKey == $"scheduled:{subscription.Id}:{cycle.Id}");

                    if (existingItem is null)
                    {
                        await GenerateItemForSubscriptionAsync(subscription, cycle, triggeredBy, cancellationToken);
                        result.ItemsGenerated++;
                    }
                    else if (existingItem.Status is BillingItemStatus.Pending or BillingItemStatus.Partial)
                    {
                        var hasPostedCharge = await _context.JournalEntries.AsNoTracking()
                            .AnyAsync(entry => entry.IdempotencyKey == $"charge:{existingItem.Id}", cancellationToken);
                        var effectiveDate = subscription.NextBillingDate ?? subscription.StartDate;
                        var terms = SubscriptionPricingRules.ResolveTerms(subscription.TermsVersions, effectiveDate);
                        var pricing = SubscriptionPricingRules.Calculate(terms.BasePrice, terms.DiscountPercent, terms.TaxPercent,
                            SubscriptionPricingRules.FirstPeriodFactor(subscription, terms));
                        if (hasPostedCharge && (existingItem.Amount != pricing.Total || existingItem.Currency != terms.Currency))
                        {
                            throw new InvalidOperationException("The charge is already posted. Reverse it and issue a new charge instead of changing its financial amount.");
                        }
                        existingItem.SubscriptionTermsVersionId = terms.Id;
                        existingItem.BaseAmount = pricing.BaseAmount;
                        existingItem.DiscountAmount = pricing.DiscountAmount;
                        existingItem.TaxAmount = pricing.TaxAmount;
                        existingItem.ProrationFactor = pricing.ProrationFactor;
                        existingItem.Amount = pricing.Total;
                        existingItem.Description = BuildDescription(subscription, terms, cycle);
                        existingItem.DueDate = CalculateDueDate(subscription, terms);
                        existingItem.Currency = terms.Currency;
                        _context.BillingItems.Update(existingItem);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Subscription {subscription.Code}: {ex.Message}");
                }
            }

            cycle.Status = originalCycleStatus == BillingCycleStatus.Closed ? BillingCycleStatus.Closed : BillingCycleStatus.Open;
            _context.BillingCycles.Update(cycle);

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result.Success = result.Errors.Count == 0;
            await FinishLogAsync(log, result, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
            cycle.Status = originalCycleStatus;
            _context.BillingCycles.Update(cycle);
            await _context.SaveChangesAsync(cancellationToken);
            result.Success = false;
            result.Errors.Add($"Error general: {ex.Message}");
            await FinishLogAsync(log, result, cancellationToken);
            return result;
        }
    }

    private async Task GenerateItemForSubscriptionAsync(Subscription subscription, BillingCycle cycle, Guid? generatedBy, CancellationToken cancellationToken)
    {
        var exists = await _context.BillingItems
            .AsNoTracking()
            .AnyAsync(bi => bi.IdempotencyKey == $"scheduled:{subscription.Id}:{cycle.Id}", cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe un cargo para esta suscripción en el ciclo.");
        }

        var trackedSubscription = await _context.Subscriptions
            .Include(s => s.TermsVersions)
            .FirstOrDefaultAsync(s => s.Id == subscription.Id, cancellationToken);

        if (trackedSubscription is null)
        {
            throw new InvalidOperationException("No se pudo cargar la suscripción para actualizar.");
        }

        var effectiveDate = subscription.NextBillingDate ?? subscription.StartDate;
        var terms = SubscriptionPricingRules.ResolveTerms(subscription.TermsVersions, effectiveDate);
        var pricing = SubscriptionPricingRules.Calculate(terms.BasePrice, terms.DiscountPercent, terms.TaxPercent,
            SubscriptionPricingRules.FirstPeriodFactor(subscription, terms));
        var billingItem = new BillingItem
        {
            Id = Guid.NewGuid(),
            Type = BillingItemType.SubscriptionCharge,
            IdempotencyKey = $"scheduled:{subscription.Id}:{cycle.Id}",
            SubscriptionId = subscription.Id,
            BillingCycleId = cycle.Id,
            ClientId = subscription.ClientId,
            SubscriptionTermsVersionId = terms.Id,
            Description = BuildDescription(subscription, terms, cycle),
            BaseAmount = pricing.BaseAmount,
            DiscountAmount = pricing.DiscountAmount,
            TaxAmount = pricing.TaxAmount,
            ProrationFactor = pricing.ProrationFactor,
            Amount = pricing.Total,
            Currency = terms.Currency,
            DueDate = CalculateDueDate(subscription, terms),
            PeriodStart = cycle.StartDate,
            PeriodEnd = cycle.EndDate,
            Status = BillingItemStatus.Pending,
            PaidAmount = 0,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = generatedBy
        };

        _context.BillingItems.Add(billingItem);
        await _majorLedger.PostChargeAsync(billingItem, cancellationToken);

        var lastBillingDate = subscription.NextBillingDate ?? subscription.StartDate;
        trackedSubscription.LastBillingDate = lastBillingDate;
        trackedSubscription.ServiceVersionId = terms.ServiceVersionId;
        trackedSubscription.BillingType = terms.BillingType;
        trackedSubscription.Price = pricing.Total;
        trackedSubscription.Currency = terms.Currency;
        trackedSubscription.DiscountPercent = terms.DiscountPercent;
        trackedSubscription.TaxPercent = terms.TaxPercent;
        trackedSubscription.BillingDay = terms.BillingDay;
        trackedSubscription.CustomIntervalDays = terms.CustomIntervalDays;
        trackedSubscription.ProrationPolicy = terms.ProrationPolicy;
        trackedSubscription.ContractTerms = terms.Terms;

        if (trackedSubscription.BillingType == BillingType.OneTime)
        {
            trackedSubscription.NextBillingDate = null;
        }
        else if (trackedSubscription.BillingType is BillingType.Monthly or BillingType.Yearly or BillingType.Custom)
        {
            trackedSubscription.NextBillingDate = SubscriptionDateCalculator.CalculateNextBillingDate(
                lastBillingDate,
                trackedSubscription.BillingType,
                trackedSubscription.BillingDay,
                trackedSubscription.CustomIntervalDays);
        }
    }

    private static string BuildDescription(Subscription subscription, SubscriptionTermsVersion terms, BillingCycle cycle)
    {
        var serviceName = subscription.Service?.Name ?? subscription.ServiceId.ToString();
        return $"{serviceName} · {subscription.Code} · {cycle.StartDate:yyyy-MM-dd}–{cycle.EndDate:yyyy-MM-dd} · {terms.BillingType} · condiciones v{terms.VersionNumber}";
    }

    private static DateTime CalculateDueDate(Subscription subscription, SubscriptionTermsVersion terms)
    {
        if (!subscription.NextBillingDate.HasValue)
        {
            throw new InvalidOperationException("La suscripción no tiene fecha de facturación.");
        }

        var date = subscription.NextBillingDate.Value;
        var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
        var day = Math.Min(Math.Max(terms.BillingDay, 1), daysInMonth);

        return new DateTime(date.Year, date.Month, day, 0, 0, 0, date.Kind);
    }

    private static (DateTime StartDate, DateTime EndDate) CalculateCycleDates(int year, int? month)
    {
        if (month.HasValue)
        {
            var start = new DateTime(year, month.Value, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(year, month.Value, DateTime.DaysInMonth(year, month.Value), 23, 59, 59, DateTimeKind.Utc);
            return (start, end);
        }

        var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var yearEnd = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        return (yearStart, yearEnd);
    }

    private async Task<BillingCycle> FindOrCreateCycleAsync(int year, int? month, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var cycle = await _context.BillingCycles
            .FirstOrDefaultAsync(c => c.Year == year && c.Month == month, cancellationToken);

        if (cycle is not null)
        {
            if (cycle.Status == BillingCycleStatus.Closed)
                throw new InvalidOperationException("El ciclo de facturación está cerrado.");
            return cycle;
        }

        cycle = new BillingCycle
        {
            Id = Guid.NewGuid(),
            Year = year,
            Month = month,
            StartDate = startDate,
            EndDate = endDate,
            Status = BillingCycleStatus.Open
        };

        _context.BillingCycles.Add(cycle);
        return cycle;
    }

    private async Task<BillingGenerationLog> StartLogAsync(Guid? cycleId, Guid? triggeredBy, CancellationToken cancellationToken)
    {
        var log = new BillingGenerationLog
        {
            Id = Guid.NewGuid(),
            BillingCycleId = cycleId,
            StartedAt = DateTime.UtcNow,
            Status = "InProgress",
            ItemsGenerated = 0,
            TriggeredBy = triggeredBy
        };

        _context.BillingGenerationLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
        return log;
    }

    private async Task FinishLogAsync(BillingGenerationLog log, BillingGenerationResult result, CancellationToken cancellationToken)
    {
        log.FinishedAt = DateTime.UtcNow;
        log.ItemsGenerated = result.ItemsGenerated;

        if (!result.Success)
        {
            log.Status = result.ItemsGenerated > 0 ? "Partial" : "Failed";
        }
        else
        {
            log.Status = "Success";
        }

        log.Errors = result.Errors.Count > 0 ? string.Join("\n", result.Errors) : null;

        _context.BillingGenerationLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
