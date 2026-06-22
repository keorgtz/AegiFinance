using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Commands.CreateClientTag;

public class CreateClientTagCommandHandler : IRequestHandler<CreateClientTagCommand, ClientTagDto>
{
    private readonly IApplicationDbContext _context;

    public CreateClientTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientTagDto> Handle(CreateClientTagCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.ClientTags
            .AsNoTracking()
            .AnyAsync(t => t.Name == request.Name, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe una etiqueta con ese nombre.");
        }

        var tag = new ClientTag
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Color = request.Color
        };

        _context.ClientTags.Add(tag);
        await _context.SaveChangesAsync(cancellationToken);

        return new ClientTagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color
        };
    }
}
