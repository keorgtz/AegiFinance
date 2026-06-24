using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AegiFinance.Web.Services;

public class BlazorPermissionService : IPermissionService
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public BlazorPermissionService(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> IsAdminAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        var userType = user.FindFirst("UserType")?.Value;
        var hasAdminRole = user.HasClaim(ClaimTypes.Role, "Admin");
        return userType == "Administrator" || hasAdminRole;
    }

    public async Task<bool> HasPermissionAsync(string permissionCode)
    {
        if (await IsAdminAsync())
        {
            return true; // Admins have all permissions in this SVA design
        }

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return false;
        }

        // Check if the user has the explicit permission claim
        var hasExplicitPermission = user.HasClaim(c => c.Type == "Permission" && c.Value == permissionCode);
        
        return hasExplicitPermission;
    }
}
