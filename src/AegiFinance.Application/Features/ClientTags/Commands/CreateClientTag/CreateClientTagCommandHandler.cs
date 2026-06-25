using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Commands.CreateClientTag;

public class CreateClientTagCommandHandler : IRequestHandler<CreateClientTagCommand, ClientTagDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateClientTagCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientTagDto> Handle(CreateClientTagCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para crear etiquetas.");
        }

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
