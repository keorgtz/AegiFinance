using MediatR;

namespace AegiFinance.Application.Features.ClientUsers.Commands.SetClientUserPin;

public class SetClientUserPinCommand : IRequest
{
    public Guid Id { get; set; }
    public string Pin { get; set; } = string.Empty;
}
