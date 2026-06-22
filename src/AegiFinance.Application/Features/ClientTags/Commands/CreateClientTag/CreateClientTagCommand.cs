using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientTags.Commands.CreateClientTag;

public class CreateClientTagCommand : IRequest<ClientTagDto>
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
