using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Queries.GetClientContacts;

public class GetClientContactsQuery : IRequest<List<ClientContactDto>>
{
    public Guid ClientId { get; set; }

    public GetClientContactsQuery() { }

    public GetClientContactsQuery(Guid clientId)
    {
        ClientId = clientId;
    }
}
