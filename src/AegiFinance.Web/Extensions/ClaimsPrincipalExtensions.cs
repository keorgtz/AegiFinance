using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AegiFinance.Web.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.Parse(value!);
    }
}
