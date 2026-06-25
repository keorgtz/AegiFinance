using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Commands.UpdateClientUser;

public class UpdateClientUserCommandHandler : IRequestHandler<UpdateClientUserCommand, ClientUserDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateClientUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientUserDto> Handle(UpdateClientUserCommand request, CancellationToken cancellationToken)
    {
        var clientUser = await _context.ClientUsers
            .FirstOrDefaultAsync(cu => cu.Id == request.Id, cancellationToken);

        if (clientUser is null)
        {
            throw new InvalidOperationException("El subusuario no existe.");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == clientUser.UserId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("El usuario asociado no existe.");
        }

        var emailTaken = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != user.Id && u.Email == request.Email, cancellationToken);

        if (emailTaken)
        {
            throw new InvalidOperationException("Ya existe un usuario con el mismo correo electrónico.");
        }

        clientUser.DisplayName = request.DisplayName;
        user.Name = request.Name;
        user.Email = request.Email;

        await _context.SaveChangesAsync(cancellationToken);

        var hasPin = await _context.ClientPinCredentials
            .AsNoTracking()
            .AnyAsync(p => p.UserId == user.Id, cancellationToken);

        return new ClientUserDto
        {
            Id = clientUser.Id,
            ClientId = clientUser.ClientId,
            UserId = user.Id,
            DisplayName = clientUser.DisplayName,
            IsActive = clientUser.IsActive,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            HasPin = hasPin
        };
    }
}
