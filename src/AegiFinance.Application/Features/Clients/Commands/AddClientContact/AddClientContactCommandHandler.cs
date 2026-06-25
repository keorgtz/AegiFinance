using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.AddClientContact;

public class AddClientContactCommandHandler : IRequestHandler<AddClientContactCommand, ClientContactDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddClientContactCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientContactDto> Handle(AddClientContactCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para agregar contactos de cliente.");
        }

        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        if (request.IsPrimary)
        {
            var existingPrimary = await _context.ClientContacts
                .Where(c => c.ClientId == request.ClientId && c.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var contact in existingPrimary)
            {
                contact.IsPrimary = false;
            }
        }

        var newContact = new ClientContact
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Position = request.Position,
            IsPrimary = request.IsPrimary
        };

        _context.ClientContacts.Add(newContact);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClientContactDto
        {
            Id = newContact.Id,
            ClientId = newContact.ClientId,
            Name = newContact.Name,
            Email = newContact.Email,
            Phone = newContact.Phone,
            Position = newContact.Position,
            IsPrimary = newContact.IsPrimary
        };
    }
}
