using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientTags.Commands.UpdateClientTag;

public class UpdateClientTagCommand : IRequest<ClientTagDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}
