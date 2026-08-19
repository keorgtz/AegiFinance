using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;

var debitAccount = Guid.NewGuid();
var creditAccount = Guid.NewGuid();
var entry = Entry(
    new JournalLine { Id = Guid.NewGuid(), AccountId = debitAccount, Debit = 125.50m },
    new JournalLine { Id = Guid.NewGuid(), AccountId = creditAccount, Credit = 125.50m });
JournalEntryRules.ValidateForPosting(entry);

AssertThrows(() => JournalEntryRules.ValidateForPosting(Entry(
    new JournalLine { Id = Guid.NewGuid(), AccountId = debitAccount, Debit = 100m },
    new JournalLine { Id = Guid.NewGuid(), AccountId = creditAccount, Credit = 99m })), "unbalanced entry");

AssertThrows(() => JournalEntryRules.ValidateForPosting(Entry(
    new JournalLine { Id = Guid.NewGuid(), AccountId = debitAccount, Debit = 100m, Credit = 1m },
    new JournalLine { Id = Guid.NewGuid(), AccountId = creditAccount, Credit = 99m })), "line with debit and credit");

entry.Status = JournalEntryStatus.Posted;
entry.EntryNumber = "JE-ORIGINAL";
entry.Description = "Original";
var reversal = JournalEntryRules.CreateReversal(entry, Guid.NewGuid(), "JE-REVERSAL", "reversal:test", DateTime.UtcNow, Guid.NewGuid());
if (reversal.Lines.ElementAt(0).Credit != 125.50m || reversal.Lines.ElementAt(1).Debit != 125.50m)
    throw new InvalidOperationException("Failed: reversal does not swap debit and credit.");
if (reversal.ReversesJournalEntryId != entry.Id || reversal.SourceType != JournalSourceType.Reversal)
    throw new InvalidOperationException("Failed: reversal trace is incomplete.");
if (reversal.Lines.Any(line => line.LegacyLedgerEntryId.HasValue))
    throw new InvalidOperationException("Failed: reversal duplicates a unique legacy migration link.");

var sourceBank = new BankAccount { Id = Guid.NewGuid(), Name = "Source", Currency = "MXN", IsActive = true };
var destinationBank = new BankAccount { Id = Guid.NewGuid(), Name = "Destination", Currency = "MXN", IsActive = true };
BankAccountRules.ValidateTransfer(sourceBank, destinationBank, 500m, "MXN");
AssertThrows(() => BankAccountRules.ValidateTransfer(sourceBank, sourceBank, 500m, "MXN"), "same-account transfer");
destinationBank.IsActive = false;
AssertThrows(() => BankAccountRules.ValidateTransfer(sourceBank, destinationBank, 500m, "MXN"), "transfer to inactive account");
destinationBank.IsActive = true;
destinationBank.Currency = "USD";
AssertThrows(() => BankAccountRules.ValidateTransfer(sourceBank, destinationBank, 500m, "MXN"), "cross-currency transfer without conversion");

var transferJournal = Entry(
    new JournalLine { Id = Guid.NewGuid(), AccountId = destinationBank.Id, Debit = 500m, BankAccountId = destinationBank.Id },
    new JournalLine { Id = Guid.NewGuid(), AccountId = sourceBank.Id, Credit = 500m, BankAccountId = sourceBank.Id });
JournalEntryRules.ValidateForPosting(transferJournal);
if (transferJournal.Lines.Sum(line => line.Debit - line.Credit) != 0)
    throw new InvalidOperationException("Failed: transfer created an artificial result.");

var normalizedCompany = ClientIdentityRules.Normalize("  Compañía del Norte, S.A. de C.V. ");
if (normalizedCompany != ClientIdentityRules.Normalize("COMPANIA DEL NORTE SA DE CV"))
    throw new InvalidOperationException("Failed: duplicate-name normalization is not accent and punctuation invariant.");
if (ClientIdentityRules.NormalizeOptional(" ab-c 123 ") != "ABC123")
    throw new InvalidOperationException("Failed: tax identifier normalization is unstable.");
if (ClientIdentityRules.NormalizeOptional("   ") is not null)
    throw new InvalidOperationException("Failed: empty duplicate identifiers must remain absent.");

var price = SubscriptionPricingRules.Calculate(1000m, 10m, 16m);
if (price.BaseAmount != 1000m || price.DiscountAmount != 100m || price.TaxAmount != 144m || price.Total != 1044m)
    throw new InvalidOperationException("Failed: subscription discount and tax order is incorrect.");

var sameDayVersion = new ServiceVersion
{
    Id = Guid.NewGuid(),
    IsPublished = true,
    EffectiveFrom = new DateTime(2026, 8, 19, 18, 30, 0, DateTimeKind.Utc)
};
if (ServiceVersionRules.ResolveApplicable([sameDayVersion], new DateTime(2026, 8, 19))?.Id != sameDayVersion.Id)
    throw new InvalidOperationException("Failed: a plan version published later on the same business date was rejected.");

var futureVersion = new ServiceVersion
{
    Id = Guid.NewGuid(),
    IsPublished = true,
    EffectiveFrom = new DateTime(2026, 8, 20, 0, 0, 0, DateTimeKind.Utc)
};
if (ServiceVersionRules.ResolveApplicable([futureVersion], new DateTime(2026, 8, 19)) is not null)
    throw new InvalidOperationException("Failed: a future plan version was accepted early.");

var subscriptionId = Guid.NewGuid();
var currentTerms = new SubscriptionTermsVersion { Id = Guid.NewGuid(), SubscriptionId = subscriptionId, VersionNumber = 1, EffectiveFrom = new DateTime(2026, 1, 1), EffectiveTo = new DateTime(2026, 7, 1), BasePrice = 100m };
var futureTerms = new SubscriptionTermsVersion { Id = Guid.NewGuid(), SubscriptionId = subscriptionId, VersionNumber = 2, EffectiveFrom = new DateTime(2026, 7, 1), BasePrice = 200m };
if (SubscriptionPricingRules.ResolveTerms([currentTerms, futureTerms], new DateTime(2026, 6, 30)).Id != currentTerms.Id)
    throw new InvalidOperationException("Failed: a future plan change altered prior terms.");
if (SubscriptionPricingRules.ResolveTerms([currentTerms, futureTerms], new DateTime(2026, 7, 1)).Id != futureTerms.Id)
    throw new InvalidOperationException("Failed: scheduled terms were not activated on their effective date.");

var proratedSubscription = new Subscription { StartDate = new DateTime(2026, 4, 16) };
var proratedTerms = new SubscriptionTermsVersion { BillingType = BillingType.Monthly, ProrationPolicy = ProrationPolicy.Daily };
if (SubscriptionPricingRules.FirstPeriodFactor(proratedSubscription, proratedTerms) != 0.5m)
    throw new InvalidOperationException("Failed: daily first-period proration is incorrect.");

Console.WriteLine("Major Ledger, bank-account, client-identity and subscription pricing rules passed.");

static JournalEntry Entry(params JournalLine[] lines) => new()
{
    Id = Guid.NewGuid(),
    Status = JournalEntryStatus.Draft,
    Lines = lines
};

static void AssertThrows(Action action, string scenario)
{
    try { action(); }
    catch (InvalidOperationException) { return; }
    throw new InvalidOperationException($"Failed: {scenario} was accepted.");
}
