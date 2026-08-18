namespace AegiFinance.Domain.Security;

public sealed record PermissionOverrideDecision(
    string Code,
    bool IsGranted,
    Guid? ClientId = null,
    Guid? SubscriptionId = null,
    DateTime? ExpiresAt = null);

public static class PermissionDecisionEngine
{
    public static IReadOnlyList<string> Resolve(
        IEnumerable<string> roleGrants,
        IEnumerable<PermissionOverrideDecision> overrides,
        Guid? clientId,
        Guid? subscriptionId,
        DateTime utcNow)
    {
        var effective = roleGrants.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in overrides.Where(item =>
                     (!item.ExpiresAt.HasValue || item.ExpiresAt > utcNow) &&
                     (!item.ClientId.HasValue || item.ClientId == clientId) &&
                     (!item.SubscriptionId.HasValue || item.SubscriptionId == subscriptionId)))
        {
            if (item.IsGranted) effective.Add(item.Code); else effective.Remove(item.Code);
        }

        return effective.OrderBy(code => code, StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
