using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Queries.GetClientUsers;

public class GetClientUsersQuery : IRequest<List<ClientUserDto>>
{
    public Guid? ClientId { get; set; }
}
