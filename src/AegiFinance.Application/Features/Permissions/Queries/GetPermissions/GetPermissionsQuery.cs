using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Permissions.Queries.GetPermissions;

public class GetPermissionsQuery : IRequest<List<PermissionDto>>
{
}
