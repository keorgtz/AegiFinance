using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.AddClientNote;

public class AddClientNoteCommandHandler : IRequestHandler<AddClientNoteCommand, ClientNoteDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AddClientNoteCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientNoteDto> Handle(AddClientNoteCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para agregar notas de cliente.");
        }

        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ClientId, cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        var note = new ClientNote
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            Content = request.Content,
            IsPinned = request.IsPinned
        };

        _context.ClientNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClientNoteDto
        {
            Id = note.Id,
            ClientId = note.ClientId,
            Content = note.Content,
            IsPinned = note.IsPinned,
            CreatedAt = note.CreatedAt
        };
    }
}
