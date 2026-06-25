using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Commands.DeleteClientTag;

public class DeleteClientTagCommandHandler : IRequestHandler<DeleteClientTagCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteClientTagCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteClientTagCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para eliminar etiquetas.");
        }

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
