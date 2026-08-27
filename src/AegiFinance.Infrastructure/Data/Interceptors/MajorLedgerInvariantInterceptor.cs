using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AegiFinance.Infrastructure.Data.Interceptors;

public sealed class MajorLedgerInvariantInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            await ValidateAsync(context, cancellationToken);
        }
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static async Task ValidateAsync(DbContext context, CancellationToken cancellationToken)
    {
        foreach (var tracked in context.ChangeTracker.Entries<AccountingPeriod>().Where(item => item.State == EntityState.Modified))
        {
            var originalStatus = tracked.OriginalValues.GetValue<AccountingPeriodStatus>(nameof(AccountingPeriod.Status));
            if (originalStatus == tracked.Entity.Status) continue;
            if (originalStatus == AccountingPeriodStatus.Open && tracked.Entity.Status == AccountingPeriodStatus.Closed)
            {
                if (!tracked.Entity.ClosedAt.HasValue || !tracked.Entity.ClosedBy.HasValue || string.IsNullOrWhiteSpace(tracked.Entity.CloseVerificationCode) || string.IsNullOrWhiteSpace(tracked.Entity.CloseChecklistJson))
                    throw new InvalidOperationException("Closing an accounting period requires verified checklist evidence.");
                continue;
            }
            if (originalStatus == AccountingPeriodStatus.Closed && tracked.Entity.Status == AccountingPeriodStatus.Open)
            {
                var approval = context.ChangeTracker.Entries<AccountingPeriodReopenRequest>().Any(item =>
                    item.Entity.AccountingPeriodId == tracked.Entity.Id && item.Entity.Status == AccountingPeriodReopenStatus.Approved &&
                    item.Entity.ReviewedAt.HasValue && item.Entity.ReviewedBy.HasValue && item.Entity.RequestedBy != item.Entity.ReviewedBy.Value);
                if (!approval) throw new InvalidOperationException("Reopening an accounting period requires independent approval evidence.");
                continue;
            }
            throw new InvalidOperationException("The accounting period status transition is not allowed.");
        }

        foreach (var tracked in context.ChangeTracker.Entries<JournalEntry>())
        {
            if (tracked.State == EntityState.Deleted && tracked.Entity.Status != JournalEntryStatus.Draft)
                throw new InvalidOperationException("Posted journal entries cannot be deleted.");

            if (tracked.State == EntityState.Modified)
            {
                var originalStatus = tracked.OriginalValues.GetValue<JournalEntryStatus>(nameof(JournalEntry.Status));
                if (originalStatus == JournalEntryStatus.Posted)
                {
                    var allowed = new HashSet<string>(StringComparer.Ordinal)
                    {
                        nameof(JournalEntry.Status), nameof(JournalEntry.ReversedAt), nameof(JournalEntry.ReversedBy),
                        nameof(JournalEntry.UpdatedAt), nameof(JournalEntry.UpdatedBy)
                    };
                    if (tracked.Properties.Any(property => property.IsModified && !allowed.Contains(property.Metadata.Name)) ||
                        tracked.Entity.Status != JournalEntryStatus.Reversed)
                        throw new InvalidOperationException("A posted journal entry is immutable and can only be reversed.");
                }
                else if (originalStatus == JournalEntryStatus.Reversed)
                {
                    throw new InvalidOperationException("A reversed journal entry is immutable.");
                }
            }

            var becomesPosted = tracked.Entity.Status == JournalEntryStatus.Posted &&
                (tracked.State == EntityState.Added || tracked.OriginalValues.GetValue<JournalEntryStatus>(nameof(JournalEntry.Status)) == JournalEntryStatus.Draft);
            if (becomesPosted) JournalEntryRules.ValidateForPosting(tracked.Entity);

            if (tracked.State == EntityState.Added || becomesPosted)
            {
                var trackedPeriod = context.ChangeTracker.Entries<AccountingPeriod>().FirstOrDefault(item => item.Entity.Id == tracked.Entity.AccountingPeriodId)?.Entity;
                var status = trackedPeriod?.Status ?? await context.Set<AccountingPeriod>().AsNoTracking()
                    .Where(item => item.Id == tracked.Entity.AccountingPeriodId).Select(item => item.Status).SingleAsync(cancellationToken);
                if (status != AccountingPeriodStatus.Open) throw new InvalidOperationException("Entries cannot be created or posted in a closed accounting period.");
            }
        }

        foreach (var tracked in context.ChangeTracker.Entries<JournalLine>()
                     .Where(entry => entry.State is EntityState.Modified or EntityState.Deleted))
        {
            var parentStatus = tracked.Entity.JournalEntry?.Status;
            if (parentStatus is null)
            {
                parentStatus = await context.Set<JournalEntry>().AsNoTracking()
                    .Where(entry => entry.Id == tracked.Entity.JournalEntryId)
                    .Select(entry => entry.Status).SingleAsync(cancellationToken);
            }
            if (parentStatus != JournalEntryStatus.Draft)
                throw new InvalidOperationException("Lines belonging to a posted or reversed journal entry are immutable.");
        }
    }
}
