using AegiFinance.Application.Dtos;
using MediatR;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<UserDto>
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public Guid? ClientId { get; set; }
}
