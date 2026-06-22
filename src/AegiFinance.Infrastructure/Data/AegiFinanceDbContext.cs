using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Data;

public class AegiFinanceDbContext : DbContext
{
    public AegiFinanceDbContext(DbContextOptions<AegiFinanceDbContext> options)
        : base(options)
    {
    }

    protected AegiFinanceDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
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
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<ServicePriceHistory> ServicePriceHistories => Set<ServicePriceHistory>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPriceHistory> SubscriptionPriceHistories => Set<SubscriptionPriceHistory>();
    public DbSet<SubscriptionChangeLog> SubscriptionChangeLogs => Set<SubscriptionChangeLog>();
    public DbSet<BillingCycle> BillingCycles => Set<BillingCycle>();
    public DbSet<BillingItem> BillingItems => Set<BillingItem>();
    public DbSet<BillingGenerationLog> BillingGenerationLogs => Set<BillingGenerationLog>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<LedgerAllocation> LedgerAllocations => Set<LedgerAllocation>();
    public DbSet<TransferGroup> TransferGroups => Set<TransferGroup>();
    public DbSet<SubscriptionAllocation> SubscriptionAllocations => Set<SubscriptionAllocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUser(modelBuilder);
        ConfigureRole(modelBuilder);
        ConfigurePermission(modelBuilder);
        ConfigureUserPermission(modelBuilder);
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
        ConfigureService(modelBuilder);
        ConfigureServiceCategory(modelBuilder);
        ConfigureServicePriceHistory(modelBuilder);
        ConfigureSubscription(modelBuilder);
        ConfigureSubscriptionPriceHistory(modelBuilder);
        ConfigureSubscriptionChangeLog(modelBuilder);
        ConfigureBillingCycle(modelBuilder);
        ConfigureBillingItem(modelBuilder);
        ConfigureBillingGenerationLog(modelBuilder);
        ConfigureBankAccount(modelBuilder);
        ConfigureLedgerEntry(modelBuilder);
        ConfigureLedgerAllocation(modelBuilder);
        ConfigureTransferGroup(modelBuilder);
        ConfigureSubscriptionAllocation(modelBuilder);

        ApplySoftDeleteQueryFilters(modelBuilder);
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
            entity.Property(e => e.RefreshToken).IsRequired(false);

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

            entity.HasMany(u => u.UserPermissions)
                .WithOne()
                .HasForeignKey(up => up.UserId);
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

            entity.HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    j => j.HasOne<Permission>().WithMany().HasForeignKey("PermissionId"),
                    j => j.HasOne<Role>().WithMany().HasForeignKey("RoleId"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId");
                        j.ToTable("RolePermissions");
                    });
        });
    }

    private static void ConfigurePermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.Code).IsUnique();
        });
    }

    private static void ConfigureUserPermission(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.PermissionId).IsRequired();

            entity.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();
        });
    }

    private static void ConfigureClientUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
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

            entity.HasIndex(e => e.Code).IsUnique();

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

            entity.HasIndex(e => e.Code).IsUnique();

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

            entity.HasIndex(e => e.Name).IsUnique();
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

            entity.HasMany(s => s.PriceHistory)
                .WithOne(ph => ph.Subscription)
                .HasForeignKey(ph => ph.SubscriptionId);

            entity.HasMany(s => s.ChangeLogs)
                .WithOne(cl => cl.Subscription)
                .HasForeignKey(cl => cl.SubscriptionId);
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
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.DueDate).IsRequired();
            entity.Property(e => e.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (BillingItemStatus)Enum.Parse(typeof(BillingItemStatus), v));
            entity.Property(e => e.GeneratedAt).IsRequired();

            entity.HasIndex(e => new { e.SubscriptionId, e.BillingCycleId }).IsUnique();
            entity.HasIndex(e => new { e.ClientId, e.Status });
            entity.HasIndex(e => e.DueDate);

            entity.HasOne(bi => bi.BillingCycle)
                .WithMany()
                .HasForeignKey(bi => bi.BillingCycleId)
                .IsRequired();

            entity.HasOne(bi => bi.Subscription)
                .WithMany()
                .HasForeignKey(bi => bi.SubscriptionId)
                .IsRequired();

            entity.HasOne(bi => bi.Client)
                .WithMany()
                .HasForeignKey(bi => bi.ClientId)
                .IsRequired();
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
            entity.Property(e => e.AccountNumber).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(3).IsRequired().HasDefaultValue("MXN");
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.OpeningDate).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
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

    private static void ConfigureLedgerAllocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LedgerAllocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasIndex(e => new { e.LedgerEntryId, e.BillingItemId }).IsUnique();

            entity.HasOne(la => la.LedgerEntry)
                .WithMany()
                .HasForeignKey(la => la.LedgerEntryId)
                .IsRequired();

            entity.HasOne(la => la.BillingItem)
                .WithMany()
                .HasForeignKey(la => la.BillingItemId)
                .IsRequired();
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

    private static void ConfigureSubscriptionAllocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionAllocation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.AllocatedAt).IsRequired();
            entity.HasIndex(e => new { e.LedgerEntryId, e.BillingItemId });

            entity.HasOne(e => e.LedgerEntry)
                .WithMany()
                .HasForeignKey(e => e.LedgerEntryId)
                .IsRequired();

            entity.HasOne(e => e.BillingItem)
                .WithMany()
                .HasForeignKey(e => e.BillingItemId)
                .IsRequired();
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
}
