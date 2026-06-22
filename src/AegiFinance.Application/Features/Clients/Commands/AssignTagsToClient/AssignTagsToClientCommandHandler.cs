using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.AssignTagsToClient;

public class AssignTagsToClientCommandHandler : IRequestHandler<AssignTagsToClientCommand>
{
    private readonly IApplicationDbContext _context;

    public AssignTagsToClientCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AssignTagsToClientCommand request, CancellationToken cancellationToken)
    {
        var client = await _context.Clients
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        var tagIds = request.TagIds.Distinct().ToList();

        var tags = await _context.ClientTags
            .Where(t => tagIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        if (tags.Count != tagIds.Count)
        {
            throw new InvalidOperationException("Una o más etiquetas no existen.");
        }

        client.Tags.Clear();
        foreach (var tag in tags)
        {
            client.Tags.Add(tag);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
