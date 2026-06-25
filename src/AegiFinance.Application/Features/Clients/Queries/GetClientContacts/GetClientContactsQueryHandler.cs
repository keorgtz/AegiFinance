using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Queries.GetClientContacts;

public class GetClientContactsQueryHandler : IRequestHandler<GetClientContactsQuery, List<ClientContactDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientContactsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientContactDto>> Handle(GetClientContactsQuery request, CancellationToken cancellationToken)
    {
        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        return await _context.ClientContacts
            .AsNoTracking()
            .Where(c => c.ClientId == request.ClientId)
            .OrderByDescending(c => c.IsPrimary)
            .ThenBy(c => c.Name)
            .Select(c => new ClientContactDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Position = c.Position,
                IsPrimary = c.IsPrimary
            })
            .ToListAsync(cancellationToken);
    }
}
