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

var receivable = new BillingItem { Amount = 1000m, PaidAmount = 200m };
receivable.Adjustments.Add(new BillingAdjustment { Type = BillingAdjustmentType.CreditNote, Amount = 100m });
receivable.Adjustments.Add(new BillingAdjustment { Type = BillingAdjustmentType.LateFee, Amount = 50m });
if (ReceivableRules.Balance(receivable) != 750m)
    throw new InvalidOperationException("Failed: receivable balance does not include credits, fees and payments.");
AssertThrows(() => ReceivableRules.ValidateCredit(receivable, 751m), "credit note above outstanding balance");

var importAccountId = Guid.NewGuid();
var importHash = BankImportRules.DeduplicationHash(importAccountId, new DateTime(2026, 8, 26), "Pago cliente 42", "REF-42", 1250.50m, "mxn", 1);
var repeatedHash = BankImportRules.DeduplicationHash(importAccountId, new DateTime(2026, 8, 26, 23, 59, 0), " Pago cliente 42 ", "ref-42", 1250.5m, "MXN", 1);
if (importHash != repeatedHash)
    throw new InvalidOperationException("Failed: bank import deduplication hash is not stable.");
if (importHash == BankImportRules.DeduplicationHash(importAccountId, new DateTime(2026, 8, 26), "Pago cliente 42", "REF-42", 1250.50m, "MXN", 2))
    throw new InvalidOperationException("Failed: legitimate repeated rows collapse into one import hash.");

var reconciliationSettings = new ReconciliationSettings();
var reconciliationScore = ReconciliationRules.Score(
    1250m, new DateTime(2026, 8, 27), "Pago Compañía Norte", "REF-900",
    1250m, new DateTime(2026, 8, 27), "Cobro mensual", "ref 900",
    "Compania Norte", reconciliationSettings);
if (!reconciliationScore.AmountExact || !reconciliationScore.ReferenceExact || reconciliationScore.Score < reconciliationSettings.SuggestionThreshold)
    throw new InvalidOperationException("Failed: an exact reconciliation does not reach the configured suggestion threshold.");
if (!reconciliationScore.Factors.Any(item => item.Code == "amount") || !reconciliationScore.Factors.Any(item => item.Code == "reference"))
    throw new InvalidOperationException("Failed: reconciliation score does not explain its amount and reference evidence.");

var invalidThresholds = new ReconciliationSettings { SuggestionThreshold = 90m, AutoConfirmThreshold = 80m };
AssertThrows(() => ReconciliationRules.ValidateSettings(invalidThresholds), "automatic threshold below suggestion threshold");
AssertThrows(() => ReconciliationRules.ValidateDifference(5m, 0.01m, ReconciliationDifferenceType.None, null), "unclassified reconciliation difference");
ReconciliationRules.ValidateDifference(5m, 0.01m, ReconciliationDifferenceType.Commission, "Comisión identificada en estado bancario");
if (ReconciliationRules.SignedAmount(new LedgerEntry { EntryType = LedgerEntryType.Expense, Amount = 100m }) != -100m ||
    ReconciliationRules.SignedAmount(new LedgerEntry { EntryType = LedgerEntryType.Income, Amount = 100m }) != 100m)
    throw new InvalidOperationException("Failed: reconciliation directions do not follow cash flow signs.");

var payment = new LedgerEntry { EntryType = LedgerEntryType.Income, Amount = 500m, ClientId = Guid.NewGuid(), Reference = "SUB-001" };
PaymentApplicationRules.ValidatePayment(payment);
AssertThrows(() => PaymentApplicationRules.ValidatePayment(new LedgerEntry { EntryType = LedgerEntryType.Expense, Amount = 500m, ClientId = payment.ClientId }), "expense used as payment");
PaymentApplicationRules.ValidateAllocation(300m, 500m, 300m);
AssertThrows(() => PaymentApplicationRules.ValidateAllocation(301m, 500m, 300m), "application above charge balance");
AssertThrows(() => PaymentApplicationRules.ValidateAllocation(501m, 500m, 600m), "application above payment balance");
var matchingService = new Service { Code = "PLAN", Name = "Plan" };
var matchingSubscription = new Subscription { Code = "SUB-001", Service = matchingService };
var matchingCharge = new BillingItem { IdempotencyKey = "charge:1", Description = "Mensualidad", Subscription = matchingSubscription };
if (PaymentApplicationRules.ReferenceScore(payment, matchingCharge) != 2)
    throw new InvalidOperationException("Failed: exact subscription reference was not prioritized.");

Console.WriteLine("Major Ledger, bank-account, client-identity, subscription pricing, bank-import, reconciliation and payment-application rules passed.");

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
