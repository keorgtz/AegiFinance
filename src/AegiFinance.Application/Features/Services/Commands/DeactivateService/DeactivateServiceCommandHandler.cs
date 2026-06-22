using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.DeactivateService;

public class DeactivateServiceCommandHandler : IRequestHandler<DeactivateServiceCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        service.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
