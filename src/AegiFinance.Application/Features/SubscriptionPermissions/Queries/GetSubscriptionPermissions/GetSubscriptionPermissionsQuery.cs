using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Queries.GetSubscriptionPermissions;

public class GetSubscriptionPermissionsQuery : IRequest<List<SubscriptionPermissionDto>>
{
    public Guid UserId { get; set; }

    public GetSubscriptionPermissionsQuery() { }

    public GetSubscriptionPermissionsQuery(Guid userId)
    {
        UserId = userId;
    }
}
