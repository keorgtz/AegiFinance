namespace AegiFinance.Web.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permissionCode);
    Task<bool> IsAdminAsync();
}

public class MockPermissionService : IPermissionService
{
    // Mock configuration for SVA demonstration.
    // In a real implementation, this would read from AuthenticationStateProvider (Claims).
    public bool IsAdmin { get; set; } = true;

    public Task<bool> HasPermissionAsync(string permissionCode)
    {
        if (IsAdmin) return Task.FromResult(true);

        // For a client, maybe they can only view certain things:
        var clientPermissions = new[] { 
            "ViewDashboard", 
            "ViewOwnAccountStatement",
            "ViewOwnSubscriptions"
        };

        return Task.FromResult(clientPermissions.Contains(permissionCode));
    }

    public Task<bool> IsAdminAsync()
    {
        return Task.FromResult(IsAdmin);
    }
}
