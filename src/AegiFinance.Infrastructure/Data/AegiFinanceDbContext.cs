using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AegiFinance.Infrastructure.Data;

public class AegiFinanceDbContext : DbContext
{
    protected virtual bool IsClientScope => false;
    protected virtual Guid? CurrentClientId => null;
    protected virtual Guid? CurrentOrganizationId => null;
    protected virtual bool IsOrganizationScope => CurrentOrganizationId.HasValue;

    public AegiFinanceDbContext(DbContextOptions<AegiFinanceDbContext> options)
        : base(options)
    {
    }

    protected AegiFinanceDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<PermissionDefinition> Permissions => Set<PermissionDefinition>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermissionOverride> UserPermissionOverrides => Set<UserPermissionOverride>();
    public DbSet<UiControlDefinition> UiControlDefinitions => Set<UiControlDefinition>();
    public DbSet<UiControlPolicy> UiControlPolicies => Set<UiControlPolicy>();
    public DbSet<ClientUser> ClientUsers => Set<ClientUser>();
    public DbSet<ClientPinCredential> ClientPinCredentials => Set<ClientPinCredential>();
    public DbSet<SubscriptionPermission> SubscriptionPermissions => Set<SubscriptionPermission>();
    public DbSet<CurrencyConfig> CurrencyConfigs => Set<CurrencyConfig>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<ClientTag> ClientTags => Set<ClientTag>();
    public DbSet<ClientCategory> ClientCategories => Set<ClientCategory>();
    public DbSet<ClientNote> ClientNotes => Set<ClientNote>();
    public DbSet<ClientContact> ClientContacts => Set<ClientContact>();
    public DbSet<ClientDocument> ClientDocuments => Set<ClientDocument>();
    public DbSet<ClientDuplicateRule> ClientDuplicateRules => Set<ClientDuplicateRule>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServicePriceHistory> ServicePriceHistories => Set<ServicePriceHistory>();
    public DbSet<ServiceVersion> ServiceVersions => Set<ServiceVersion>();
    public DbSet<ServiceVersionConcept> ServiceVersionConcepts => Set<ServiceVersionConcept>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionTermsVersion> SubscriptionTermsVersions => Set<SubscriptionTermsVersion>();
    public DbSet<SubscriptionRenewal> SubscriptionRenewals => Set<SubscriptionRenewal>();
    public DbSet<SubscriptionPriceHistory> SubscriptionPriceHistories => Set<SubscriptionPriceHistory>();
    public DbSet<SubscriptionChangeLog> SubscriptionChangeLogs => Set<SubscriptionChangeLog>();
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();
    public DbSet<BillingItem> BillingItems => Set<BillingItem>();
    public DbSet<BillingAdjustment> BillingAdjustments => Set<BillingAdjustment>();
    public DbSet<PaymentPromise> PaymentPromises => Set<PaymentPromise>();
    public DbSet<BillingGenerationLog> BillingGenerationLogs => Set<BillingGenerationLog>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<GeneralLedgerAccount> GeneralLedgerAccounts => Set<GeneralLedgerAccount>();
    public DbSet<AccountingPeriod> AccountingPeriods => Set<AccountingPeriod>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<TransferGroup> TransferGroups => Set<TransferGroup>();
    public DbSet<SubscriptionAllocation> SubscriptionAllocations => Set<SubscriptionAllocation>();
    public DbSet<PaymentApplication> PaymentApplications => Set<PaymentApplication>();
    public DbSet<PaymentApplicationPayment> PaymentApplicationPayments => Set<PaymentApplicationPayment>();
    public DbSet<PaymentApplicationSettings> PaymentApplicationSettings => Set<PaymentApplicationSettings>();
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<BankImportAttempt> BankImportAttempts => Set<BankImportAttempt>();
    public DbSet<BankImportProfile> BankImportProfiles => Set<BankImportProfile>();
    public DbSet<BankImportRow> BankImportRows => Set<BankImportRow>();
    public DbSet<ReconciliationSettings> ReconciliationSettings => Set<ReconciliationSettings>();
    public DbSet<ReconciliationCase> ReconciliationCases => Set<ReconciliationCase>();
    public DbSet<ReconciliationCaseBankLine> ReconciliationCaseBankLines => Set<ReconciliationCaseBankLine>();
    public DbSet<ReconciliationCaseLedgerEntry> ReconciliationCaseLedgerEntries => Set<ReconciliationCaseLedgerEntry>();
    public DbSet<ReconciliationPeriod> ReconciliationPeriods => Set<ReconciliationPeriod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOrganization(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureUserSession(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureUserPermission(modelBuilder);
        ConfigureUiControlDefinition(modelBuilder);
        ConfigureUiControlPolicy(modelBuilder);
        ConfigureClientUser(modelBuilder);
        ConfigureClientPinCredential(modelBuilder);
        ConfigureSubscriptionPermission(modelBuilder);
        ConfigureCurrencyConfig(modelBuilder);
        ConfigureExchangeRate(modelBuilder);
        ConfigureAuditLog(modelBuilder);
        ConfigureClient(modelBuilder);
        ConfigureClientTag(modelBuilder);
        ConfigureClientCategory(modelBuilder);
        ConfigureClientNote(modelBuilder);
        ConfigureClientContact(modelBuilder);
        ConfigureClientDocument(modelBuilder);
        ConfigureClientDuplicateRule(modelBuilder);
        ConfigureService(modelBuilder);
        ConfigureServiceCategory(modelBuilder);
        ConfigureServicePriceHistory(modelBuilder);
        ConfigureServiceVersion(modelBuilder);
        ConfigureServiceVersionConcept(modelBuilder);
        ConfigureSubscription(modelBuilder);
        ConfigureSubscriptionTermsVersion(modelBuilder);
        ConfigureSubscriptionRenewal(modelBuilder);
        ConfigureSubscriptionPriceHistory(modelBuilder);
        ConfigureSubscriptionChangeLog(modelBuilder);
        ConfigureBillingCycle(modelBuilder);
        ConfigureBillingItem(modelBuilder);
        ConfigureBillingAdjustment(modelBuilder);
        ConfigurePaymentPromise(modelBuilder);
        ConfigureBillingGenerationLog(modelBuilder);
        ConfigureBankAccount(modelBuilder);
        ConfigureLedgerEntry(modelBuilder);
        ConfigureGeneralLedgerAccount(modelBuilder);
        ConfigureAccountingPeriod(modelBuilder);
        ConfigureJournalEntry(modelBuilder);
        ConfigureJournalLine(modelBuilder);
        ConfigureTransferGroup(modelBuilder);
        ConfigureSubscriptionAllocation(modelBuilder);
        ConfigurePaymentApplications(modelBuilder);
        ConfigureBankStatement(modelBuilder);
        ConfigureBankStatementLine(modelBuilder);
        ConfigureBankImportAttempt(modelBuilder);
        ConfigureBankImportProfile(modelBuilder);
        ConfigureBankImportRow(modelBuilder);
        ConfigureReconciliation(modelBuilder);

        ApplySoftDeleteQueryFilters(modelBuilder);
        ApplyTenantQueryFilters(modelBuilder);
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.UserType)
                .HasConversion(
                    v => v.ToString(),
                    v => (UserType)Enum.Parse(typeof(UserType), v));
            entity.Property(e => e.ClientId).IsRequired(false);
            entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId)
                .IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j => j.HasOne<User>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("UserRoles");
                    });

            entity.HasMany(u => u.PermissionOverrides)
                .WithOne(up => up.User)
                .HasForeignKey(up => up.UserId);

            entity.HasMany(u => u.UiControlPolicies)
                .WithOne(policy => policy.User)
                .HasForeignKey(policy => policy.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(50).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(item => item.Code).IsUnique();
        });
    }

    private static void ConfigureUserSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.RefreshTokenHash).HasMaxLength(200).IsRequired();
            entity.Property(item => item.DeviceName).HasMaxLength(120);
            entity.Property(item => item.IpAddress).HasMaxLength(64);
            entity.Property(item => item.UserAgent).HasMaxLength(500);
            entity.Property(item => item.RevokedReason).HasMaxLength(300);
            entity.HasIndex(item => new { item.UserId, item.ExpiresAt });
            entity.HasOne(item => item.User).WithMany(user => user.Sessions)
                .HasForeignKey(item => item.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureRole(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UserType)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToString() : null,
                    v => string.IsNullOrEmpty(v) ? null : (UserType?)Enum.Parse(typeof(UserType), v));

            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId);

            entity.HasMany(r => r.UiControlPolicies)
                .WithOne(policy => policy.Role)
                .HasForeignKey(policy => policy.RoleId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            entity.Property(e => e.IsGranted).HasDefaultValue(true);
            entity.HasOne(e => e.Permission)
                .WithMany(permission => permission.RolePermissions)
                .HasForeignKey(e => e.PermissionId);
        });

        AppendQueryFilter<RolePermission>(modelBuilder, entity => !entity.Permission.IsDeleted);
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PermissionDefinition>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Kind)
                .HasConversion(
                    value => value.ToString(),
                    value => (PermissionKind)Enum.Parse(typeof(PermissionKind), value));
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private static void ConfigureUserPermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPermissionOverride>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PermissionId).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.PermissionId })
                .IsUnique().HasFilter("[ClientId] IS NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.UserId, e.PermissionId, e.ClientId })
                .IsUnique().HasFilter("[ClientId] IS NOT NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.UserId, e.PermissionId, e.SubscriptionId })
                .IsUnique().HasFilter("[SubscriptionId] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasOne(e => e.Permission)
                .WithMany(permission => permission.UserPermissionOverrides)
                .HasForeignKey(e => e.PermissionId);
        });
    }

    private static void ConfigureUiControlDefinition(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UiControlDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ControlKey).HasMaxLength(240).IsRequired();
            entity.Property(e => e.Label).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Module).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ControlType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RequiredPermissionCode).HasMaxLength(100);
            entity.HasIndex(e => e.ControlKey).IsUnique();
        });
    }

    private static void ConfigureUiControlPolicy(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UiControlPolicy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccessMode)
                .HasConversion(
                    value => value.ToString(),
                    value => (UiAccessMode)Enum.Parse(typeof(UiAccessMode), value));
            entity.HasOne(e => e.UiControlDefinition)
                .WithMany(definition => definition.Policies)
                .HasForeignKey(e => e.UiControlDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UiControlDefinitionId, e.RoleId })
                .IsUnique().HasFilter("[RoleId] IS NOT NULL AND [UserId] IS NULL AND [ClientId] IS NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.UiControlDefinitionId, e.UserId })
                .IsUnique().HasFilter("[UserId] IS NOT NULL AND [RoleId] IS NULL AND [ClientId] IS NULL AND [SubscriptionId] IS NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.UiControlDefinitionId, e.RoleId, e.ClientId, e.SubscriptionId })
                .IsUnique().HasFilter("[RoleId] IS NOT NULL AND [UserId] IS NULL AND [IsDeleted] = 0");
            entity.HasIndex(e => new { e.UiControlDefinitionId, e.UserId, e.ClientId, e.SubscriptionId })
                .IsUnique().HasFilter("[UserId] IS NOT NULL AND [RoleId] IS NULL AND [IsDeleted] = 0");
        });
    }

    private static void ConfigureClientUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
            entity.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureClientPinCredential(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientPinCredential>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PinHash).IsRequired();

            entity.HasIndex(e => e.UserId).IsUnique();
        });
    }

    private static void ConfigureSubscriptionPermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCurrencyConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CurrencyConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(10).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private static void ConfigureExchangeRate(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.RateToMXN).HasPrecision(18, 6);
            entity.Property(e => e.RateFromMXN).HasPrecision(18, 6);
            entity.Property(e => e.Source)
                .HasConversion(
                    v => v.ToString(),
                    v => (ExchangeRateSource)Enum.Parse(typeof(ExchangeRateSource), v));
        });
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Changes).IsRequired();
            entity.Property(e => e.IPAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });
    }

    private static void ConfigureClient(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TradeName).HasMaxLength(200);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.BillingEmail).HasMaxLength(256);
            entity.Property(e => e.BillingAddress).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (ClientStatus)Enum.Parse(typeof(ClientStatus), v));
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.PresentationCurrency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.CommercialTerms).HasMaxLength(2000);
            entity.Property(e => e.NormalizedName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NormalizedTaxId).HasMaxLength(50);
            entity.Property(e => e.NormalizedBillingEmail).HasMaxLength(256);

            entity.HasIndex(e => new { e.OrganizationId, e.Code }).IsUnique();
            entity.HasIndex(e => new { e.OrganizationId, e.NormalizedTaxId });
            entity.HasIndex(e => new { e.OrganizationId, e.NormalizedName });

            entity.HasOne(e => e.Organization).WithMany(item => item.Clients)
                .HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.AccountManagerUser).WithMany()
                .HasForeignKey(e => e.AccountManagerUserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Category)
                .WithMany(c => c.Clients)
                .HasForeignKey(c => c.CategoryId)
                .IsRequired(false);

            entity.HasMany(c => c.Tags)
                .WithMany(t => t.Clients)
                .UsingEntity<Dictionary<string, object>>(
                    "ClientClientTag",
                    j => j.HasOne<ClientTag>().WithMany().HasForeignKey("TagId"),
                    j => j.HasOne<Client>().WithMany().HasForeignKey("ClientId"),
                    j =>
                    {
                        j.HasKey("ClientId", "TagId");
                        j.ToTable("ClientClientTag");
                    });

            entity.HasMany(c => c.NotesList)
                .WithOne(n => n.Client)
                .HasForeignKey(n => n.ClientId);

            entity.HasMany(c => c.Contacts)
                .WithOne(con => con.Client)
                .HasForeignKey(con => con.ClientId);
        });
    }

    private static void ConfigureClientDocument(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientDocument>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(260).IsRequired();
            entity.Property(item => item.StorageKey).HasMaxLength(500).IsRequired();
            entity.Property(item => item.ContentType).HasMaxLength(150).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(500);
            entity.HasIndex(item => new { item.ClientId, item.CreatedAt });
            entity.HasOne(item => item.Client).WithMany(client => client.Documents)
                .HasForeignKey(item => item.ClientId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureClientDuplicateRule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientDuplicateRule>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.OrganizationId).IsUnique();
            entity.HasOne(item => item.Organization).WithMany()
                .HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureClientTag(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Color).HasMaxLength(7).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private static void ConfigureClientCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.Name).IsUnique();
        });
    }

    private static void ConfigureClientNote(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientNote>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(4000).IsRequired();
        });
    }

    private static void ConfigureClientContact(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientContact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Position).HasMaxLength(100);
        });
    }

    private static void ConfigureService(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.BillingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (BillingType)Enum.Parse(typeof(BillingType), v));
            entity.Property(e => e.DefaultPrice).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");

            entity.HasIndex(e => new { e.OrganizationId, e.Code }).IsUnique();
            entity.HasOne(e => e.Organization).WithMany(e => e.Services)
                .HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Category)
                .WithMany(c => c.Services)
                .HasForeignKey(s => s.CategoryId)
                .IsRequired(false);

            entity.HasMany(s => s.PriceHistory)
                .WithOne(ph => ph.Service)
                .HasForeignKey(ph => ph.ServiceId);
        });
    }

    private static void ConfigureServiceCategory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
            entity.HasOne(e => e.Organization).WithMany(e => e.ServiceCategories)
                .HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureServicePriceHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServicePriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.EffectiveDate).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(ph => ph.Service)
                .WithMany(s => s.PriceHistory)
                .HasForeignKey(ph => ph.ServiceId);
        });
    }

    private static void ConfigureServiceVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.BillingType).HasConversion<string>();
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.DefaultDiscountPercent).HasPrecision(9, 4);
            entity.Property(e => e.DefaultTaxPercent).HasPrecision(9, 4);
            entity.Property(e => e.ProrationPolicy).HasConversion<string>();
            entity.Property(e => e.Terms).HasMaxLength(4000);
            entity.HasIndex(e => new { e.ServiceId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => new { e.ServiceId, e.EffectiveFrom });
            entity.HasOne(e => e.Service).WithMany(e => e.Versions)
                .HasForeignKey(e => e.ServiceId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureServiceVersionConcept(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ServiceVersionConcept>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(9, 4);
            entity.HasIndex(e => new { e.ServiceVersionId, e.Code }).IsUnique();
            entity.HasOne(e => e.ServiceVersion).WithMany(e => e.Concepts)
                .HasForeignKey(e => e.ServiceVersionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSubscription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BillingType)
                .HasConversion(
                    v => v.ToString(),
                    v => (BillingType)Enum.Parse(typeof(BillingType), v));
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(9, 4);
            entity.Property(e => e.TaxPercent).HasPrecision(9, 4);
            entity.Property(e => e.ProrationPolicy).HasConversion<string>();
            entity.Property(e => e.ContractTerms).HasMaxLength(4000);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.BillingDay).IsRequired();
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (SubscriptionStatus)Enum.Parse(typeof(SubscriptionStatus), v));
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.LastBillingDate).IsRequired(false);
            entity.Property(e => e.NextBillingDate).IsRequired(false);

            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasIndex(e => new { e.ClientId, e.Status });

            entity.HasOne(s => s.Client)
                .WithMany()
                .HasForeignKey(s => s.ClientId)
                .IsRequired();

            entity.HasOne(s => s.Service)
                .WithMany()
                .HasForeignKey(s => s.ServiceId)
                .IsRequired();

            entity.HasOne(s => s.ServiceVersion).WithMany()
                .HasForeignKey(s => s.ServiceVersionId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(s => s.PriceHistory)
                .WithOne(ph => ph.Subscription)
                .HasForeignKey(ph => ph.SubscriptionId);

            entity.HasMany(s => s.ChangeLogs)
                .WithOne(cl => cl.Subscription)
                .HasForeignKey(cl => cl.SubscriptionId);
        });
    }

    private static void ConfigureSubscriptionTermsVersion(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionTermsVersion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BillingType).HasConversion<string>();
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.DiscountPercent).HasPrecision(9, 4);
            entity.Property(e => e.TaxPercent).HasPrecision(9, 4);
            entity.Property(e => e.ProrationPolicy).HasConversion<string>();
            entity.Property(e => e.Terms).HasMaxLength(4000);
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.HasIndex(e => new { e.SubscriptionId, e.VersionNumber }).IsUnique();
            entity.HasIndex(e => new { e.SubscriptionId, e.EffectiveFrom });
            entity.HasOne(e => e.Subscription).WithMany(e => e.TermsVersions)
                .HasForeignKey(e => e.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.ServiceVersion).WithMany()
                .HasForeignKey(e => e.ServiceVersionId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSubscriptionRenewal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionRenewal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.HasIndex(e => new { e.SubscriptionId, e.IdempotencyKey }).IsUnique();
            entity.HasOne(e => e.Subscription).WithMany(e => e.Renewals)
                .HasForeignKey(e => e.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureSubscriptionPriceHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPriceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OldPrice).HasPrecision(18, 2);
            entity.Property(e => e.NewPrice).HasPrecision(18, 2);
            entity.Property(e => e.EffectiveDate).IsRequired();
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(ph => ph.Subscription)
                .WithMany(s => s.PriceHistory)
                .HasForeignKey(ph => ph.SubscriptionId);
        });
    }

    private static void ConfigureSubscriptionChangeLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionChangeLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangeType)
                .HasConversion(
                    v => v.ToString(),
                    v => (SubscriptionChangeType)Enum.Parse(typeof(SubscriptionChangeType), v));
            entity.Property(e => e.OldValue).HasMaxLength(500);
            entity.Property(e => e.NewValue).HasMaxLength(500);
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(cl => cl.Subscription)
                .WithMany(s => s.ChangeLogs)
                .HasForeignKey(cl => cl.SubscriptionId);
        });
    }

    private static void ConfigureBillingCycle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingCycle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (BillingCycleStatus)Enum.Parse(typeof(BillingCycleStatus), v));
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();

            entity.HasIndex(e => new { e.Year, e.Month }).IsUnique();
        });
    }

    private static void ConfigureBillingItem(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.BaseAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ProrationFactor).HasPrecision(12, 8);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentAllocationVersion).IsConcurrencyToken();
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (BillingItemStatus)Enum.Parse(typeof(BillingItemStatus), v));
            entity.Property(e => e.GeneratedAt).IsRequired();

            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(160).IsRequired();
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => new { e.SubscriptionId, e.BillingCycleId });
            entity.HasIndex(e => new { e.ClientId, e.Status });
            entity.HasIndex(e => e.DueDate);

            entity.HasOne(bi => bi.BillingCycle)
                .WithMany()
                .HasForeignKey(bi => bi.BillingCycleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(bi => bi.Subscription)
                .WithMany()
                .HasForeignKey(bi => bi.SubscriptionId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(bi => bi.Client)
                .WithMany()
                .HasForeignKey(bi => bi.ClientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(bi => bi.SubscriptionTermsVersion)
                .WithMany()
                .HasForeignKey(bi => bi.SubscriptionTermsVersionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBillingAdjustment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingAdjustment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(24);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Reason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.IdempotencyKey).HasMaxLength(160).IsRequired();
            entity.Property(e => e.ReversalReason).HasMaxLength(500);
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasOne(e => e.BillingItem).WithMany(e => e.Adjustments)
                .HasForeignKey(e => e.BillingItemId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePaymentPromise(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentPromise>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PromisedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(24);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasIndex(e => new { e.BillingItemId, e.Status, e.PromiseDate });
            entity.HasOne(e => e.BillingItem).WithMany(e => e.PaymentPromises)
                .HasForeignKey(e => e.BillingItemId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBillingGenerationLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BillingGenerationLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Errors).HasMaxLength(4000);
            entity.Property(e => e.StartedAt).IsRequired();

            entity.HasOne(bgl => bgl.BillingCycle)
                .WithMany()
                .HasForeignKey(bgl => bgl.BillingCycleId)
                .IsRequired(false);
        });
    }

    private static void ConfigureBankAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.BankName).HasMaxLength(200);
            entity.Property(e => e.AccountNumber).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.OpeningDate).IsRequired();

            entity.HasIndex(e => new { e.OrganizationId, e.Name }).IsUnique();
            entity.HasOne(e => e.Organization).WithMany(item => item.BankAccounts)
                .HasForeignKey(e => e.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureLedgerEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntryType)
                .HasConversion(
                    v => v.ToString(),
                    v => (LedgerEntryType)Enum.Parse(typeof(LedgerEntryType), v));
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(200);
            entity.Property(e => e.ReconciliationVersion).IsConcurrencyToken();
            entity.Property(e => e.PaymentAllocationVersion).IsConcurrencyToken();

            entity.HasIndex(e => new { e.BankAccountId, e.Date });

            entity.HasOne(le => le.BankAccount)
                .WithMany()
                .HasForeignKey(le => le.BankAccountId)
                .IsRequired();

            entity.HasOne(le => le.Client)
                .WithMany()
                .HasForeignKey(le => le.ClientId)
                .IsRequired(false);

            entity.HasOne(le => le.BillingItem)
                .WithMany()
                .HasForeignKey(le => le.BillingItemId)
                .IsRequired(false);
        });
    }

    private static void ConfigureTransferGroup(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransferGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(tg => tg.FromEntry)
                .WithMany()
                .HasForeignKey(tg => tg.FromEntryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(tg => tg.ToEntry)
                .WithMany()
                .HasForeignKey(tg => tg.ToEntryId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureGeneralLedgerAccount(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneralLedgerAccount>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Code).HasMaxLength(30).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.Property(item => item.AccountType).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Purpose).HasConversion<string>().HasMaxLength(40);
            entity.HasIndex(item => item.Code).IsUnique();
            entity.HasIndex(item => item.BankAccountId).IsUnique().HasFilter("[BankAccountId] IS NOT NULL AND [IsDeleted] = 0");
            entity.HasOne(item => item.ParentAccount).WithMany().HasForeignKey(item => item.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.BankAccount).WithMany().HasForeignKey(item => item.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAccountingPeriod(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountingPeriod>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(item => new { item.StartDate, item.EndDate }).IsUnique();
        });
    }

    private static void ConfigureJournalEntry(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.EntryNumber).HasMaxLength(40).IsRequired();
            entity.Property(item => item.Description).HasMaxLength(500).IsRequired();
            entity.Property(item => item.Reference).HasMaxLength(200);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.SourceId).HasMaxLength(100);
            entity.Property(item => item.IdempotencyKey).HasMaxLength(160).IsRequired();
            entity.HasIndex(item => item.EntryNumber).IsUnique();
            entity.HasIndex(item => item.IdempotencyKey).IsUnique();
            entity.HasIndex(item => new { item.Date, item.Status });
            entity.HasOne(item => item.AccountingPeriod).WithMany().HasForeignKey(item => item.AccountingPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Client).WithMany().HasForeignKey(item => item.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.ReversesJournalEntry).WithMany().HasForeignKey(item => item.ReversesJournalEntryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureJournalLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JournalLine>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Debit).HasPrecision(18, 2);
            entity.Property(item => item.Credit).HasPrecision(18, 2);
            entity.Property(item => item.Description).HasMaxLength(500);
            entity.HasIndex(item => new { item.AccountId, item.JournalEntryId });
            entity.HasIndex(item => item.LegacyLedgerEntryId)
                .IsUnique().HasFilter("[LegacyLedgerEntryId] IS NOT NULL AND [IsDeleted] = 0");
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_JournalLines_DebitOrCredit", "([Debit] > 0 AND [Credit] = 0) OR ([Credit] > 0 AND [Debit] = 0)");
            });
            entity.HasOne(item => item.JournalEntry).WithMany(entry => entry.Lines)
                .HasForeignKey(item => item.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Account).WithMany().HasForeignKey(item => item.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Client).WithMany().HasForeignKey(item => item.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.BankAccount).WithMany().HasForeignKey(item => item.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.BillingItem).WithMany().HasForeignKey(item => item.BillingItemId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.LegacyLedgerEntry).WithMany().HasForeignKey(item => item.LegacyLedgerEntryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureSubscriptionAllocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionAllocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.AllocatedAt).IsRequired();
            entity.HasIndex(e => new { e.LedgerEntryId, e.BillingItemId });
            entity.HasIndex(e => new { e.PaymentApplicationId, e.IsReversed });
            entity.Property(e => e.ReversalReason).HasMaxLength(1000);

            entity.HasOne(e => e.LedgerEntry)
                .WithMany()
                .HasForeignKey(e => e.LedgerEntryId)
                .IsRequired();

            entity.HasOne(e => e.BillingItem)
                .WithMany()
                .HasForeignKey(e => e.BillingItemId)
                .IsRequired();

            entity.HasOne(e => e.PaymentApplication)
                .WithMany(application => application.Allocations)
                .HasForeignKey(e => e.PaymentApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePaymentApplications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentApplication>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ReceiptNumber).HasMaxLength(50).IsRequired();
            entity.Property(item => item.IdempotencyKey).HasMaxLength(100).IsRequired();
            entity.Property(item => item.Priority).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Origin).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.Property(item => item.TotalPaymentAmount).HasPrecision(18, 2);
            entity.Property(item => item.AppliedAmount).HasPrecision(18, 2);
            entity.Property(item => item.UnappliedAmount).HasPrecision(18, 2);
            entity.Property(item => item.ReversalReason).HasMaxLength(1000);
            entity.HasIndex(item => new { item.OrganizationId, item.ReceiptNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(item => new { item.OrganizationId, item.IdempotencyKey }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(item => new { item.ClientId, item.AppliedAt });
            entity.HasOne(item => item.Organization).WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Client).WithMany().HasForeignKey(item => item.ClientId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.PreferredService).WithMany().HasForeignKey(item => item.PreferredServiceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.ReappliesPaymentApplication).WithMany().HasForeignKey(item => item.ReappliesPaymentApplicationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentApplicationPayment>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.AvailableBefore).HasPrecision(18, 2);
            entity.Property(item => item.AppliedAmount).HasPrecision(18, 2);
            entity.Property(item => item.UnappliedAfter).HasPrecision(18, 2);
            entity.HasIndex(item => new { item.PaymentApplicationId, item.LedgerEntryId }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(item => item.PaymentApplication).WithMany(application => application.Payments).HasForeignKey(item => item.PaymentApplicationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.LedgerEntry).WithMany().HasForeignKey(item => item.LedgerEntryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.JournalEntry).WithMany().HasForeignKey(item => item.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PaymentApplicationSettings>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.DefaultPriority).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(item => item.OrganizationId).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(item => item.Organization).WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBankStatement(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankStatement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StatementDate).IsRequired();
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.ClosingBalance).HasPrecision(18, 2);
            entity.Property(e => e.FileUrl).HasMaxLength(1000);
            entity.Property(e => e.FileHash).HasMaxLength(64);
            entity.HasIndex(e => new { e.BankAccountId, e.FileHash })
                .HasFilter("[FileHash] IS NOT NULL AND [IsDeleted] = 0");

            entity.HasOne(bs => bs.BankAccount)
                .WithMany()
                .HasForeignKey(bs => bs.BankAccountId)
                .IsRequired();
                
            entity.HasMany(bs => bs.Lines)
                .WithOne(bsl => bsl.BankStatement)
                .HasForeignKey(bsl => bsl.BankStatementId);
        });
    }

    private static void ConfigureBankStatementLine(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankStatementLine>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(200);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired();
            entity.Property(e => e.BankBalance).HasPrecision(18, 2);
            entity.Property(e => e.DeduplicationHash).HasMaxLength(64);
            entity.Property(e => e.ReconciliationVersion).IsConcurrencyToken();
            entity.HasIndex(e => e.DeduplicationHash).IsUnique()
                .HasFilter("[DeduplicationHash] IS NOT NULL AND [IsDeleted] = 0");
            
            entity.HasOne(bsl => bsl.LedgerEntry)
                .WithMany()
                .HasForeignKey(bsl => bsl.LedgerEntryId)
                .IsRequired(false);
        });
    }

    private static void ConfigureBankImportAttempt(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankImportAttempt>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.FileName).HasMaxLength(260).IsRequired();
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(item => item.Error).HasMaxLength(2000);
            entity.Property(item => item.FileHash).HasMaxLength(64).IsRequired();
            entity.Property(item => item.FileType).HasMaxLength(20).IsRequired();
            entity.Property(item => item.AdapterCode).HasMaxLength(50).IsRequired();
            entity.Property(item => item.AttemptedAt).IsRequired();
            entity.HasIndex(item => new { item.Status, item.AttemptedAt });
            entity.HasIndex(item => new { item.BankAccountId, item.FileHash });
            entity.HasOne(item => item.BankAccount)
                .WithMany()
                .HasForeignKey(item => item.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Profile)
                .WithMany()
                .HasForeignKey(item => item.ProfileId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(item => item.BankStatement)
                .WithMany()
                .HasForeignKey(item => item.BankStatementId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBankImportProfile(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankImportProfile>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Name).HasMaxLength(120).IsRequired();
            entity.Property(item => item.AdapterCode).HasMaxLength(50).IsRequired();
            entity.Property(item => item.DateColumn).HasMaxLength(120).IsRequired();
            entity.Property(item => item.DescriptionColumn).HasMaxLength(120).IsRequired();
            entity.Property(item => item.ReferenceColumn).HasMaxLength(120);
            entity.Property(item => item.AmountColumn).HasMaxLength(120);
            entity.Property(item => item.DebitColumn).HasMaxLength(120);
            entity.Property(item => item.CreditColumn).HasMaxLength(120);
            entity.Property(item => item.CurrencyColumn).HasMaxLength(120);
            entity.Property(item => item.BalanceColumn).HasMaxLength(120);
            entity.Property(item => item.DateFormat).HasMaxLength(50);
            entity.Property(item => item.Delimiter).HasMaxLength(4).IsRequired();
            entity.HasIndex(item => new { item.OrganizationId, item.Name }).IsUnique()
                .HasFilter("[IsDeleted] = 0");
            entity.HasOne(item => item.Organization).WithMany().HasForeignKey(item => item.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBankImportRow(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BankImportRow>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.RawDataJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(item => item.Description).HasMaxLength(1000);
            entity.Property(item => item.Reference).HasMaxLength(200);
            entity.Property(item => item.Amount).HasPrecision(18, 2);
            entity.Property(item => item.Currency).HasMaxLength(3);
            entity.Property(item => item.Balance).HasPrecision(18, 2);
            entity.Property(item => item.DeduplicationHash).HasMaxLength(64);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(item => item.IssuesJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.HasIndex(item => new { item.ImportAttemptId, item.RowNumber }).IsUnique()
                .HasFilter("[IsDeleted] = 0");
            entity.HasIndex(item => new { item.Status, item.DeduplicationHash });
            entity.HasOne(item => item.ImportAttempt).WithMany(item => item.Rows)
                .HasForeignKey(item => item.ImportAttemptId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(item => item.BankStatementLine).WithMany()
                .HasForeignKey(item => item.BankStatementLineId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReconciliation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReconciliationSettings>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.AmountTolerance).HasPrecision(18, 2);
            entity.Property(item => item.SuggestionThreshold).HasPrecision(5, 2);
            entity.Property(item => item.AutoConfirmThreshold).HasPrecision(5, 2);
            entity.HasIndex(item => item.OrganizationId).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(item => item.Organization).WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReconciliationCase>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.MatchType).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Score).HasPrecision(5, 2);
            entity.Property(item => item.ExplanationJson).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(item => item.BankAmount).HasPrecision(18, 2);
            entity.Property(item => item.LedgerAmount).HasPrecision(18, 2);
            entity.Property(item => item.DifferenceAmount).HasPrecision(18, 2);
            entity.Property(item => item.DifferenceType).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.DifferenceReason).HasMaxLength(1000);
            entity.Property(item => item.ReversalReason).HasMaxLength(1000);
            entity.HasIndex(item => new { item.BankAccountId, item.Status, item.GeneratedAt });
            entity.HasOne(item => item.Organization).WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.BankAccount).WithMany().HasForeignKey(item => item.BankAccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReconciliationCaseBankLine>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.AppliedAmount).HasPrecision(18, 2);
            entity.HasIndex(item => new { item.ReconciliationCaseId, item.BankStatementLineId }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(item => item.BankStatementLineId);
            entity.HasOne(item => item.ReconciliationCase).WithMany(item => item.BankLines).HasForeignKey(item => item.ReconciliationCaseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.BankStatementLine).WithMany().HasForeignKey(item => item.BankStatementLineId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReconciliationCaseLedgerEntry>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.AppliedAmount).HasPrecision(18, 2);
            entity.HasIndex(item => new { item.ReconciliationCaseId, item.LedgerEntryId }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(item => item.LedgerEntryId);
            entity.HasOne(item => item.ReconciliationCase).WithMany(item => item.LedgerEntries).HasForeignKey(item => item.ReconciliationCaseId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.LedgerEntry).WithMany().HasForeignKey(item => item.LedgerEntryId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ReconciliationPeriod>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Currency).HasMaxLength(3).IsRequired();
            entity.Property(item => item.BankAmount).HasPrecision(18, 2);
            entity.Property(item => item.LedgerAmount).HasPrecision(18, 2);
            entity.Property(item => item.DifferenceAmount).HasPrecision(18, 2);
            entity.Property(item => item.DifferenceType).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(item => item.Justification).HasMaxLength(1000);
            entity.HasIndex(item => new { item.BankAccountId, item.StartDate, item.EndDate }).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(item => item.Organization).WithMany().HasForeignKey(item => item.OrganizationId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.BankAccount).WithMany().HasForeignKey(item => item.BankAccountId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ApplySoftDeleteQueryFilters(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(BaseEntity).IsAssignableFrom(e.ClrType));

        foreach (var entityType in entityTypes)
        {
            var method = typeof(AegiFinanceDbContext)
                .GetMethod(nameof(SetSoftDeleteQueryFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                ?.MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, new object[] { modelBuilder });
        }
    }

    private static void SetSoftDeleteQueryFilter<TEntity>(ModelBuilder builder)
        where TEntity : BaseEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => !e.IsDeleted);
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        AppendQueryFilter<Organization>(modelBuilder, entity =>
            !IsOrganizationScope || entity.Id == CurrentOrganizationId);
        AppendQueryFilter<Client>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.Id == CurrentClientId)));
        AppendQueryFilter<BankAccount>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<User>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<ClientUser>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<ClientNote>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<ClientContact>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<ClientDocument>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<ClientDuplicateRule>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ServiceCategory>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<Service>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ServicePriceHistory>(modelBuilder, entity =>
            !IsOrganizationScope || entity.Service.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ServiceVersion>(modelBuilder, entity =>
            !IsOrganizationScope || entity.Service.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ServiceVersionConcept>(modelBuilder, entity =>
            !IsOrganizationScope || entity.ServiceVersion.Service.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<Subscription>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<SubscriptionPriceHistory>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Subscription.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.Subscription.ClientId == CurrentClientId)));
        AppendQueryFilter<SubscriptionChangeLog>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Subscription.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.Subscription.ClientId == CurrentClientId)));
        AppendQueryFilter<SubscriptionTermsVersion>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Subscription.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.Subscription.ClientId == CurrentClientId)));
        AppendQueryFilter<SubscriptionRenewal>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Subscription.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.Subscription.ClientId == CurrentClientId)));
        AppendQueryFilter<BillingItem>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<BillingAdjustment>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.BillingItem.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.BillingItem.ClientId == CurrentClientId)));
        AppendQueryFilter<PaymentPromise>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.BillingItem.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.BillingItem.ClientId == CurrentClientId)));
        AppendQueryFilter<LedgerEntry>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.BankAccount.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<JournalEntry>(modelBuilder, entity =>
            (!IsOrganizationScope ||
                (entity.ClientId.HasValue && entity.Client!.OrganizationId == CurrentOrganizationId) ||
                entity.Lines.Any(line => line.BankAccountId.HasValue && line.BankAccount!.OrganizationId == CurrentOrganizationId)) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<JournalLine>(modelBuilder, entity =>
            (!IsOrganizationScope ||
                (entity.ClientId.HasValue && entity.Client!.OrganizationId == CurrentOrganizationId) ||
                (entity.BankAccountId.HasValue && entity.BankAccount!.OrganizationId == CurrentOrganizationId) ||
                (entity.JournalEntry.ClientId.HasValue && entity.JournalEntry.Client!.OrganizationId == CurrentOrganizationId)) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<SubscriptionAllocation>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.BillingItem.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.BillingItem.ClientId == CurrentClientId)));
        AppendQueryFilter<PaymentApplication>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.ClientId == CurrentClientId)));
        AppendQueryFilter<PaymentApplicationPayment>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.PaymentApplication.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.PaymentApplication.ClientId == CurrentClientId)));
        AppendQueryFilter<PaymentApplicationSettings>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<SubscriptionPermission>(modelBuilder, entity =>
            (!IsOrganizationScope || entity.Subscription.Client.OrganizationId == CurrentOrganizationId) &&
            (!IsClientScope || (CurrentClientId.HasValue && entity.Subscription.ClientId == CurrentClientId)));
        AppendQueryFilter<BankStatement>(modelBuilder, entity =>
            !IsOrganizationScope || entity.BankAccount.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<BankStatementLine>(modelBuilder, entity =>
            !IsOrganizationScope || entity.BankStatement.BankAccount.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<BankImportAttempt>(modelBuilder, entity =>
            !IsOrganizationScope || entity.BankAccount.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<BankImportProfile>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<BankImportRow>(modelBuilder, entity =>
            !IsOrganizationScope || entity.ImportAttempt.BankAccount.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ReconciliationSettings>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ReconciliationCase>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ReconciliationCaseBankLine>(modelBuilder, entity =>
            !IsOrganizationScope || entity.ReconciliationCase.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ReconciliationCaseLedgerEntry>(modelBuilder, entity =>
            !IsOrganizationScope || entity.ReconciliationCase.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<ReconciliationPeriod>(modelBuilder, entity =>
            !IsOrganizationScope || entity.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<TransferGroup>(modelBuilder, entity =>
            !IsOrganizationScope || entity.FromEntry.BankAccount.OrganizationId == CurrentOrganizationId);
        AppendQueryFilter<UiControlPolicy>(modelBuilder, entity =>
            !IsClientScope || !entity.ClientId.HasValue ||
            (CurrentClientId.HasValue && entity.ClientId == CurrentClientId));
        AppendQueryFilter<UserPermissionOverride>(modelBuilder, entity =>
            !IsClientScope || !entity.ClientId.HasValue ||
            (CurrentClientId.HasValue && entity.ClientId == CurrentClientId));
    }

    private static void AppendQueryFilter<TEntity>(
        ModelBuilder modelBuilder,
        Expression<Func<TEntity, bool>> filter)
        where TEntity : class
    {
        var entityType = modelBuilder.Entity<TEntity>().Metadata;
        var existingFilter = entityType.GetQueryFilter();
        if (existingFilter is null)
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
            return;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var existingBody = new ParameterReplaceVisitor(existingFilter.Parameters[0], parameter)
            .Visit(existingFilter.Body)!;
        var addedBody = new ParameterReplaceVisitor(filter.Parameters[0], parameter)
            .Visit(filter.Body)!;
        var combined = Expression.Lambda<Func<TEntity, bool>>(
            Expression.AndAlso(existingBody, addedBody),
            parameter);

        modelBuilder.Entity<TEntity>().HasQueryFilter(combined);
    }

    private sealed class ParameterReplaceVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ParameterReplaceVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _source ? _target : base.VisitParameter(node);
    }
}
