using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.SetPrimaryContact;

public class SetPrimaryContactCommandHandler : IRequestHandler<SetPrimaryContactCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SetPrimaryContactCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(SetPrimaryContactCommand request, CancellationToken cancellationToken)
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

        var otherContacts = await _context.ClientContacts
            .Where(c => c.ClientId == request.ClientId && c.Id != request.ContactId && c.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var other in otherContacts)
        {
            other.IsPrimary = false;
        }

        contact.IsPrimary = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
