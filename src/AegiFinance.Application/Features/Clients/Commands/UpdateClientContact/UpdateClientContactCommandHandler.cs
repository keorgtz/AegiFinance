using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.UpdateClientContact;

public class UpdateClientContactCommandHandler : IRequestHandler<UpdateClientContactCommand, ClientContactDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClientContactCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientContactDto> Handle(UpdateClientContactCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para modificar contactos de cliente.");
        }

        var contact = await _context.ClientContacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.ClientId == request.ClientId, cancellationToken);

        if (contact is null)
        {
            throw new InvalidOperationException("El contacto no existe o no pertenece al cliente.");
        }

        if (request.IsPrimary && !contact.IsPrimary)
        {
            var otherPrimaryContacts = await _context.ClientContacts
                .Where(c => c.ClientId == request.ClientId && c.Id != request.ContactId && c.IsPrimary)
                .ToListAsync(cancellationToken);

            foreach (var other in otherPrimaryContacts)
            {
                other.IsPrimary = false;
            }
        }

        contact.Name = request.Name;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.Position = request.Position;
        contact.IsPrimary = request.IsPrimary;

        await _context.SaveChangesAsync(cancellationToken);

        return new ClientContactDto
        {
            Id = contact.Id,
            ClientId = contact.ClientId,
            Name = contact.Name,
            Email = contact.Email,
            Phone = contact.Phone,
            Position = contact.Position,
            IsPrimary = contact.IsPrimary
        };
    }
}
