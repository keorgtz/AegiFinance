using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Commands.UpdateClientUser;

public class UpdateClientUserCommand : IRequest<ClientUserDto>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
