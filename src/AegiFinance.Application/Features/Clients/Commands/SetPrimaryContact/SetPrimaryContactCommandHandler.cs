using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.SetPrimaryContact;

public class SetPrimaryContactCommandHandler : IRequestHandler<SetPrimaryContactCommand>
{
    private readonly IApplicationDbContext _context;

    public SetPrimaryContactCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SetPrimaryContactCommand request, CancellationToken cancellationToken)
    {
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
