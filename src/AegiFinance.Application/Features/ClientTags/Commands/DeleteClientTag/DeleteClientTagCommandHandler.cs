using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Commands.DeleteClientTag;

public class DeleteClientTagCommandHandler : IRequestHandler<DeleteClientTagCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteClientTagCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteClientTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await _context.ClientTags
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (tag is null)
        {
            throw new InvalidOperationException("La etiqueta no existe.");
        }

        _context.ClientTags.Remove(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
