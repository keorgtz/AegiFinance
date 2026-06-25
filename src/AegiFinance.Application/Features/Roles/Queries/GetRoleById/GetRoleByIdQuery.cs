using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQuery : IRequest<RoleDetailDto?>
{
    public Guid Id { get; set; }

    public GetRoleByIdQuery() { }

    public GetRoleByIdQuery(Guid id)
    {
        Id = id;
    }
}
