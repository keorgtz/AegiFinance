using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Common.Extensions;

public static class CurrentUserServiceExtensions
{
    public static bool IsClientUser(this ICurrentUserService currentUserService)
    {
        return Enum.TryParse<UserType>(currentUserService.UserType, out var userType) && userType == UserType.Client;
    }
}
