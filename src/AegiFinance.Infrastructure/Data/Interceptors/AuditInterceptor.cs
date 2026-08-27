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
                    auditLogs.Add(CreateAuditLog(context, entry, "Created", null, currentUserId));
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    var changes = GetChanges(entry);
                    if (changes.Any())
                    {
                        auditLogs.Add(CreateAuditLog(context, entry, "Updated", changes, currentUserId));
                    }
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = currentUserId;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                    auditLogs.Add(CreateAuditLog(context, entry, "Deleted", GetChanges(entry), currentUserId));
                    break;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<RolePermission>()
                     .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var action = entry.State switch
            {
                EntityState.Added => "Granted",
                EntityState.Deleted => "Revoked",
                _ => entry.Entity.IsGranted ? "Granted" : "Denied"
            };
            auditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = _currentUserService.OrganizationId,
                EntityType = nameof(RolePermission),
                EntityId = $"{entry.Entity.RoleId}:{entry.Entity.PermissionId}",
                Action = action,
                Changes = JsonSerializer.Serialize(new { entry.Entity.RoleId, entry.Entity.PermissionId, entry.Entity.IsGranted }),
                UserId = currentUserId,
                Timestamp = now
            });
        }

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }

        await Task.CompletedTask;
    }

    private AuditLog CreateAuditLog(DbContext context, EntityEntry<BaseEntity> entry, string action, Dictionary<string, object?>? changes, Guid? userId)
    {
        var organizationId = _currentUserService.OrganizationId ?? OrganizationFromEntity(entry.Entity);
        if (!organizationId.HasValue && entry.Entity is UserSession session)
            organizationId = context.ChangeTracker.Entries<User>()
                .FirstOrDefault(item => item.Entity.Id == session.UserId)?.Entity.OrganizationId;
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            EntityType = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id.ToString(),
            Action = action,
            Changes = changes is not null ? JsonSerializer.Serialize(changes) : "{}",
            UserId = userId ?? (entry.Entity is UserSession userSession ? userSession.UserId : null),
            Timestamp = DateTime.UtcNow,
            IPAddress = entry.Entity is UserSession sessionWithAddress ? sessionWithAddress.IpAddress : null,
            UserAgent = entry.Entity is UserSession sessionWithAgent ? sessionWithAgent.UserAgent : null
        };
    }

    private static Guid? OrganizationFromEntity(BaseEntity entity)
    {
        if (entity is Organization organization) return organization.Id;
        var property = entity.GetType().GetProperty("OrganizationId");
        if (property?.PropertyType == typeof(Guid)) return (Guid?)property.GetValue(entity);
        if (property?.PropertyType == typeof(Guid?)) return (Guid?)property.GetValue(entity);
        return null;
    }

    private static Dictionary<string, object?> GetChanges(EntityEntry<BaseEntity> entry)
    {
        var changes = new Dictionary<string, object?>();

        foreach (var property in entry.OriginalValues.Properties)
        {
            if (property.Name.Contains("Token", StringComparison.OrdinalIgnoreCase) || property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
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
