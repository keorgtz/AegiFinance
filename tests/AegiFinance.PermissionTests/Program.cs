using AegiFinance.Domain.Security;

var clientA = Guid.NewGuid();
var clientB = Guid.NewGuid();
var subscriptionA = Guid.NewGuid();
var subscriptionB = Guid.NewGuid();
var now = DateTime.UtcNow;

AssertMissing(PermissionDecisionEngine.Resolve([], [], null, null, now), "CreateClients", "role denial");
AssertMissing(PermissionDecisionEngine.Resolve(["UpdateClients"], [new("UpdateClients", false)], null, null, now), "UpdateClients", "user denial override");
AssertMissing(PermissionDecisionEngine.Resolve([], [new("ViewClients", true, clientA)], clientB, null, now), "ViewClients", "client isolation");
AssertContains(PermissionDecisionEngine.Resolve([], [new("ViewClients", true, clientA)], clientA, null, now), "ViewClients", "client-scoped grant");
AssertMissing(PermissionDecisionEngine.Resolve([], [new("ViewSubscriptions", true, clientA, subscriptionA)], clientA, subscriptionB, now), "ViewSubscriptions", "subscription isolation");
AssertContains(PermissionDecisionEngine.Resolve([], [new("ViewSubscriptions", true, clientA, subscriptionA)], clientA, subscriptionA, now), "ViewSubscriptions", "subscription-scoped grant");
AssertMissing(PermissionDecisionEngine.Resolve(["ManageUsers"], [new("ManageUsers", false)], null, null, now), "ManageUsers", "explicit denial overrides role grant");
AssertMissing(PermissionDecisionEngine.Resolve([], [new("ManageRoles", true, null, null, now.AddMinutes(-1))], null, null, now), "ManageRoles", "expired override");

var activeSession = new AegiFinance.Domain.Entities.UserSession { ExpiresAt = now.AddMinutes(1) };
if (!activeSession.IsActiveAt(now)) throw new InvalidOperationException("Failed: active session");
activeSession.RevokedAt = now;
if (activeSession.IsActiveAt(now)) throw new InvalidOperationException("Failed: revoked session");

Console.WriteLine("Permission resolution tests passed: role, user, client and subscription denial paths.");

static void AssertContains(IReadOnlyList<string> permissions, string permission, string scenario)
{
    if (!permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException($"Failed: {scenario}");
}

static void AssertMissing(IReadOnlyList<string> permissions, string permission, string scenario)
{
    if (permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException($"Failed: {scenario}");
}
