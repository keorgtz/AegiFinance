using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Queries.GetClientNotes;

public class GetClientNotesQuery : IRequest<List<ClientNoteDto>>
{
    public Guid ClientId { get; set; }

    public GetClientNotesQuery() { }

    public GetClientNotesQuery(Guid clientId)
    {
        ClientId = clientId;
    }
}
