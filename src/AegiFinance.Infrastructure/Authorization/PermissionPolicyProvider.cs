using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AegiFinance.Infrastructure.Authorization;

/// <summary>
/// Convierte cualquier nombre de policy no registrado explícitamente en un PermissionRequirement,
/// permitiendo usar [Authorize(Policy = "ManageUsers")] directamente con el código del permiso.
/// </summary>
public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var existing = await base.GetPolicyAsync(policyName);
        if (existing is not null)
        {
            return existing;
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
