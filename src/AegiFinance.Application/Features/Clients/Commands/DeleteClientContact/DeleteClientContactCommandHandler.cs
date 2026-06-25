using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.DeleteClientContact;

public class DeleteClientContactCommandHandler : IRequestHandler<DeleteClientContactCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClientContactCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteClientContactCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para eliminar contactos de cliente.");
        }

        var contact = await _context.ClientContacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId && c.ClientId == request.ClientId, cancellationToken);

        if (contact is null)
        {
            throw new InvalidOperationException("El contacto no existe o no pertenece al cliente.");
        }

        _context.ClientContacts.Remove(contact);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
