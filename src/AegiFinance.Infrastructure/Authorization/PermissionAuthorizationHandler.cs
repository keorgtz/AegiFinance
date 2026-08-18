using AegiFinance.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace AegiFinance.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IPermissionService _permissions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(
        ICurrentUserService currentUser,
        IPermissionService permissions,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _permissions = permissions;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (_currentUser.UserId is not Guid userId) return;
        var request = _httpContextAccessor.HttpContext?.Request;
        var controller = request?.RouteValues["controller"]?.ToString();
        var routeId = ParseGuid(request?.RouteValues["id"]?.ToString());
        var clientId = ParseGuid(request?.RouteValues["clientId"]?.ToString())
            ?? ParseGuid(request?.Query["clientId"].FirstOrDefault())
            ?? (string.Equals(controller, "Clients", StringComparison.OrdinalIgnoreCase) ? routeId : null)
            ?? _currentUser.ClientId;
        var subscriptionId = ParseGuid(request?.RouteValues["subscriptionId"]?.ToString())
            ?? ParseGuid(request?.Query["subscriptionId"].FirstOrDefault())
            ?? (string.Equals(controller, "Subscriptions", StringComparison.OrdinalIgnoreCase) ? routeId : null);

        if (await _permissions.HasPermissionAsync(userId, requirement.Permission, clientId, subscriptionId))
        {
            context.Succeed(requirement);
        }
    }

    private static Guid? ParseGuid(string? value) => Guid.TryParse(value, out var parsed) ? parsed : null;
}
