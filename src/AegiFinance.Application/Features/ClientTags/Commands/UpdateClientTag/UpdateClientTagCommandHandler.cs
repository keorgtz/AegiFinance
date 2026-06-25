using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Commands.UpdateClientTag;

public class UpdateClientTagCommandHandler : IRequestHandler<UpdateClientTagCommand, ClientTagDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClientTagCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientTagDto> Handle(UpdateClientTagCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para modificar etiquetas.");
        }

        var tag = await _context.ClientTags
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag is null)
        {
            throw new InvalidOperationException("La etiqueta no existe.");
        }

        var nameExists = await _context.ClientTags
            .AsNoTracking()
            .AnyAsync(t => t.Id != request.Id && t.Name == request.Name, cancellationToken);

        if (nameExists)
        {
            throw new InvalidOperationException("Ya existe otra etiqueta con ese nombre.");
        }

        tag.Name = request.Name;
        tag.Color = request.Color;

        await _context.SaveChangesAsync(cancellationToken);

        return new ClientTagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }
}
