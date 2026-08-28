using AegiFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AegiFinance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<Role> Roles { get; }
    DbSet<PermissionDefinition> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserPermissionOverride> UserPermissionOverrides { get; }
    DbSet<UiControlDefinition> UiControlDefinitions { get; }
    DbSet<UiControlPolicy> UiControlPolicies { get; }
    DbSet<ClientUser> ClientUsers { get; }
    DbSet<ClientPinCredential> ClientPinCredentials { get; }
    DbSet<SubscriptionPermission> SubscriptionPermissions { get; }
    DbSet<CurrencyConfig> CurrencyConfigs { get; }
    DbSet<ExchangeRate> ExchangeRates { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Client> Clients { get; }
    DbSet<ClientTag> ClientTags { get; }
    DbSet<ClientCategory> ClientCategories { get; }
    DbSet<ClientNote> ClientNotes { get; }
    DbSet<ClientContact> ClientContacts { get; }
    DbSet<ClientDocument> ClientDocuments { get; }
    DbSet<ClientDuplicateRule> ClientDuplicateRules { get; }
    DbSet<Service> Services { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<ServicePriceHistory> ServicePriceHistories { get; }
    DbSet<ServiceVersion> ServiceVersions { get; }
    DbSet<ServiceVersionConcept> ServiceVersionConcepts { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionTermsVersion> SubscriptionTermsVersions { get; }
    DbSet<SubscriptionRenewal> SubscriptionRenewals { get; }
    DbSet<SubscriptionPriceHistory> SubscriptionPriceHistories { get; }
    DbSet<SubscriptionChangeLog> SubscriptionChangeLogs { get; }
    DbSet<BillingCycle> BillingCycles { get; }
    DbSet<BillingItem> BillingItems { get; }
    DbSet<BillingAdjustment> BillingAdjustments { get; }
    DbSet<PaymentPromise> PaymentPromises { get; }
    DbSet<BillingGenerationLog> BillingGenerationLogs { get; }
    DbSet<BankAccount> BankAccounts { get; }
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<GeneralLedgerAccount> GeneralLedgerAccounts { get; }
    DbSet<AccountingPeriod> AccountingPeriods { get; }
    DbSet<AccountingPeriodReopenRequest> AccountingPeriodReopenRequests { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalLine> JournalLines { get; }
    DbSet<TransferGroup> TransferGroups { get; }
    DbSet<SubscriptionAllocation> SubscriptionAllocations { get; }
    DbSet<PaymentApplication> PaymentApplications { get; }
    DbSet<PaymentApplicationPayment> PaymentApplicationPayments { get; }
    DbSet<PaymentApplicationSettings> PaymentApplicationSettings { get; }
    DbSet<AccountStatementInquiry> AccountStatementInquiries { get; }
    DbSet<BankStatement> BankStatements { get; }
    DbSet<BankStatementLine> BankStatementLines { get; }
    DbSet<BankImportAttempt> BankImportAttempts { get; }
    DbSet<BankImportProfile> BankImportProfiles { get; }
    DbSet<BankImportRow> BankImportRows { get; }
    DbSet<ReconciliationSettings> ReconciliationSettings { get; }
    DbSet<ReconciliationCase> ReconciliationCases { get; }
    DbSet<ReconciliationCaseBankLine> ReconciliationCaseBankLines { get; }
    DbSet<ReconciliationCaseLedgerEntry> ReconciliationCaseLedgerEntries { get; }
    DbSet<ReconciliationPeriod> ReconciliationPeriods { get; }
    DbSet<ReportSchedule> ReportSchedules { get; }
    DbSet<ReportRun> ReportRuns { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
