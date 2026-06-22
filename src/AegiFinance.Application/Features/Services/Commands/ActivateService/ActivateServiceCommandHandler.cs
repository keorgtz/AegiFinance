using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.ActivateService;

public class ActivateServiceCommandHandler : IRequestHandler<ActivateServiceCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ActivateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (service is null)
        {
            throw new InvalidOperationException("El servicio no existe.");
        }

        service.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
