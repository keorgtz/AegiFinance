using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserDto>
{
}
