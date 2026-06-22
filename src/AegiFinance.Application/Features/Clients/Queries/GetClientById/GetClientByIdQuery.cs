using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Queries.GetClientById;

public class GetClientByIdQuery : IRequest<ClientDetailDto>
{
    public Guid Id { get; set; }

    public GetClientByIdQuery() { }

    public GetClientByIdQuery(Guid id)
    {
        Id = id;
    }
}
