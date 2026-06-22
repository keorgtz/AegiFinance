using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid Id { get; set; }
}
