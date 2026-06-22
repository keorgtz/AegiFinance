using System.Security.Claims;
using AegiFinance.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AegiFinance.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => GetGuidClaim(ClaimTypes.NameIdentifier);
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    public string? UserType => _httpContextAccessor.HttpContext?.User?.FindFirstValue("userType");
    public Guid? ClientId => GetGuidClaim("clientId");

    private Guid? GetGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(claimType);
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
