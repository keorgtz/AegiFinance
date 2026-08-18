using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Accounting;

public static class JournalEntryRules
{
    public static void ValidateForPosting(JournalEntry entry)
    {
        if (entry.Lines.Count < 2)
        {
            throw new InvalidOperationException("A journal entry requires at least two lines.");
        }

        foreach (var line in entry.Lines)
        {
            var hasDebit = line.Debit > 0;
            var hasCredit = line.Credit > 0;
            if (line.Debit < 0 || line.Credit < 0 || hasDebit == hasCredit)
            {
                throw new InvalidOperationException("Each journal line must contain either a positive debit or a positive credit.");
            }
        }

        var debit = entry.Lines.Sum(line => line.Debit);
        var credit = entry.Lines.Sum(line => line.Credit);
        if (debit != credit)
        {
            throw new InvalidOperationException($"Journal entry is out of balance. Debit: {debit}; credit: {credit}.");
        }
    }

    public static JournalEntry CreateReversal(
        JournalEntry original,
        Guid periodId,
        string entryNumber,
        string idempotencyKey,
        DateTime reversalDate,
        Guid? userId)
    {
        if (original.Status != JournalEntryStatus.Posted)
        {
            throw new InvalidOperationException("Only posted journal entries can be reversed.");
        }

        var reversal = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = entryNumber,
            Date = reversalDate,
            Description = $"Reversal of {original.EntryNumber}: {original.Description}",
            Reference = original.Reference,
            Currency = original.Currency,
            Status = JournalEntryStatus.Posted,
            SourceType = JournalSourceType.Reversal,
            SourceId = original.Id.ToString(),
            IdempotencyKey = idempotencyKey,
            AccountingPeriodId = periodId,
            ClientId = original.ClientId,
            PostedAt = DateTime.UtcNow,
            PostedBy = userId,
            ReversesJournalEntryId = original.Id,
            Lines = original.Lines.Select(line => new JournalLine
            {
                Id = Guid.NewGuid(),
                AccountId = line.AccountId,
                Debit = line.Credit,
                Credit = line.Debit,
                Description = line.Description,
                ClientId = line.ClientId,
                BankAccountId = line.BankAccountId,
                BillingItemId = line.BillingItemId
            }).ToList()
        };

        ValidateForPosting(reversal);
        return reversal;
    }
}
