using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Roles.Queries.GetRoles;

public class GetRolesQuery : IRequest<List<RoleDto>>
{
}
