using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AegiFinance.Infrastructure.Data.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await ApplyAuditsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task ApplyAuditsAsync(DbContext context, CancellationToken cancellationToken)
    {
        var entries = context.ChangeTracker.Entries<BaseEntity>().ToList();
        var auditLogs = new List<AuditLog>();
        var now = DateTime.UtcNow;
        var currentUserId = _currentUserService.UserId;

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog)
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.UpdatedBy = currentUserId;
                    auditLogs.Add(CreateAuditLog(entry, "Created", null));
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    var changes = GetChanges(entry);
                    if (changes.Any())
                    {
                        auditLogs.Add(CreateAuditLog(entry, "Updated", changes));
                    }
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = currentUserId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    auditLogs.Add(CreateAuditLog(entry, "Deleted", GetChanges(entry)));
                    break;
            }
        }

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }

        await Task.CompletedTask;
    }

    private static AuditLog CreateAuditLog(EntityEntry<BaseEntity> entry, string action, Dictionary<string, object?>? changes)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = action,
            Changes = changes is not null ? JsonSerializer.Serialize(changes) : "{}",
            Timestamp = DateTime.UtcNow
        };
    }

    private static Dictionary<string, object?> GetChanges(EntityEntry<BaseEntity> entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.OriginalValues.Properties)
        {
            var original = entry.OriginalValues[property];
            var current = entry.CurrentValues[property];

            if (!Equals(original, current))
            {
                changes[property.Name] = new { Old = original, New = current };
            }
        }

        return changes;
    }
}
