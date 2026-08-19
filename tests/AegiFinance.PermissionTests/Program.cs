using AegiFinance.Domain.Security;
using AegiFinance.Application.Features.Clients.Commands.CreateClient;
using AegiFinance.Application.Features.Roles.Commands.CreateRole;
using AegiFinance.Application.Features.Services.Commands.CreateService;
using AegiFinance.Application.Features.Subscriptions.Commands.CreateSubscription;
using AegiFinance.Application.Features.Users.Commands.CreateUser;
using AegiFinance.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

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

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
jsonOptions.Converters.Add(new JsonStringEnumConverter());
AssertJsonEnum<CreateClientCommand>("""{"status":"Active"}""", item => item.Status == ClientStatus.Active, "client status JSON contract");
AssertJsonEnum<CreateUserCommand>("""{"userType":"Administrator"}""", item => item.UserType == UserType.Administrator, "user type JSON contract");
AssertJsonEnum<CreateRoleCommand>("""{"userType":"Client"}""", item => item.UserType == UserType.Client, "role user type JSON contract");
AssertJsonEnum<CreateServiceCommand>("""{"billingType":"Monthly"}""", item => item.BillingType == BillingType.Monthly, "service billing type JSON contract");
AssertJsonEnum<CreateSubscriptionCommand>("""{"billingType":"Yearly","prorationPolicy":"Daily"}""", item => item.BillingType == BillingType.Yearly && item.ProrationPolicy == ProrationPolicy.Daily, "subscription JSON contract");

Console.WriteLine("Permission and API enum contract tests passed.");

void AssertJsonEnum<T>(string json, Func<T, bool> assertion, string scenario)
{
    var value = JsonSerializer.Deserialize<T>(json, jsonOptions);
    if (value is null || !assertion(value)) throw new InvalidOperationException($"Failed: {scenario}");
}

static void AssertContains(IReadOnlyList<string> permissions, string permission, string scenario)
{
    if (!permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException($"Failed: {scenario}");
}

static void AssertMissing(IReadOnlyList<string> permissions, string permission, string scenario)
{
    if (permissions.Contains(permission, StringComparer.OrdinalIgnoreCase)) throw new InvalidOperationException($"Failed: {scenario}");
}
