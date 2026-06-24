using AegiFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserPermission> UserPermissions { get; }
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
    DbSet<Service> Services { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<ServicePriceHistory> ServicePriceHistories { get; }
    DbSet<Subscription> Subscriptions { get; }
    DbSet<SubscriptionPriceHistory> SubscriptionPriceHistories { get; }
    DbSet<SubscriptionChangeLog> SubscriptionChangeLogs { get; }
    DbSet<BillingCycle> BillingCycles { get; }
    DbSet<BillingItem> BillingItems { get; }
    DbSet<BillingGenerationLog> BillingGenerationLogs { get; }
    DbSet<BankAccount> BankAccounts { get; }
    DbSet<LedgerEntry> LedgerEntries { get; }
    DbSet<LedgerAllocation> LedgerAllocations { get; }
    DbSet<TransferGroup> TransferGroups { get; }
    DbSet<SubscriptionAllocation> SubscriptionAllocations { get; }
    DbSet<BankStatement> BankStatements { get; }
    DbSet<BankStatementLine> BankStatementLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
