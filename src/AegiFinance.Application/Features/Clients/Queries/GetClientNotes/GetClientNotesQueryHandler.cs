using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Queries.GetClientNotes;

public class GetClientNotesQueryHandler : IRequestHandler<GetClientNotesQuery, List<ClientNoteDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientNotesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientNoteDto>> Handle(GetClientNotesQuery request, CancellationToken cancellationToken)
    {
        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        var notes = await _context.ClientNotes
            .AsNoTracking()
            .Where(n => n.ClientId == request.ClientId)
            .Select(n => new ClientNoteDto
            {
                Id = n.Id,
                ClientId = n.ClientId,
                Content = n.Content,
                IsPinned = n.IsPinned,
                CreatedAt = n.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return notes
            .OrderByDescending(n => n.IsPinned)
            .ThenByDescending(n => n.CreatedAt)
            .ToList();
    }
}
