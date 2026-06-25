using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Commands.CreateClientUser;

public class CreateClientUserCommand : IRequest<ClientUserDto>
{
    public Guid ClientId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
