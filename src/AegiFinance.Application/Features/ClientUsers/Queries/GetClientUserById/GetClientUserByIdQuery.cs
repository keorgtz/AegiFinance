using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Queries.GetClientUserById;

public class GetClientUserByIdQuery : IRequest<ClientUserDto?>
{
    public Guid Id { get; set; }

    public GetClientUserByIdQuery() { }

    public GetClientUserByIdQuery(Guid id)
    {
        Id = id;
    }
}
