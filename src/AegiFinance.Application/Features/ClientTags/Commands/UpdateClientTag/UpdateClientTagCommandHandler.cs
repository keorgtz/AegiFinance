using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Commands.UpdateClientTag;

public class UpdateClientTagCommandHandler : IRequestHandler<UpdateClientTagCommand, ClientTagDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateClientTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientTagDto> Handle(UpdateClientTagCommand request, CancellationToken cancellationToken)
    {
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
