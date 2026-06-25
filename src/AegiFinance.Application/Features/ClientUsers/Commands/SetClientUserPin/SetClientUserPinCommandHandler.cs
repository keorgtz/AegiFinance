using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Commands.SetClientUserPin;

public class SetClientUserPinCommandHandler : IRequestHandler<SetClientUserPinCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPinHasher _pinHasher;

    public SetClientUserPinCommandHandler(IApplicationDbContext context, IPinHasher pinHasher)
    {
        _context = context;
        _pinHasher = pinHasher;
    }

    public async Task Handle(SetClientUserPinCommand request, CancellationToken cancellationToken)
    {
        var clientUser = await _context.ClientUsers
            .FirstOrDefaultAsync(cu => cu.Id == request.Id, cancellationToken);

        if (clientUser is null)
        {
            throw new InvalidOperationException("El subusuario no existe.");
        }

        var credential = await _context.ClientPinCredentials
            .FirstOrDefaultAsync(p => p.UserId == clientUser.UserId, cancellationToken);

        var pinHash = _pinHasher.HashPin(request.Pin);

        if (credential is null)
        {
            credential = new ClientPinCredential
            {
                Id = Guid.NewGuid(),
                UserId = clientUser.UserId,
                PinHash = pinHash
            };

            _context.ClientPinCredentials.Add(credential);
        }
        else
        {
            credential.PinHash = pinHash;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
