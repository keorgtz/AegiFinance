using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Commands.CreateClientUser;

public class CreateClientUserCommandHandler : IRequestHandler<CreateClientUserCommand, ClientUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;

    public CreateClientUserCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
    }

    public async Task<ClientUserDto> Handle(CreateClientUserCommand request, CancellationToken cancellationToken)
    {
        var clientId = request.ClientId;

        var isClientCaller = Enum.TryParse<UserType>(_currentUserService.UserType, out var callerType)
            && callerType == UserType.Client;

        if (isClientCaller)
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                throw new UnauthorizedAccessException("No tiene un cliente asociado.");
            }

            clientId = _currentUserService.ClientId.Value;
        }

        var clientExists = await _context.Clients
            .AsNoTracking()
            .AnyAsync(c => c.Id == clientId, cancellationToken);

        if (!clientExists)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email, cancellationToken);

        if (userExists)
        {
            throw new InvalidOperationException("Ya existe un usuario con el mismo nombre de usuario o correo electrónico.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Name = request.Name,
            UserType = UserType.Client,
            ClientId = clientId,
            IsActive = true,
            EmailConfirmed = false
        };

        _context.Users.Add(user);

        var clientUser = new ClientUser
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            UserId = user.Id,
            DisplayName = request.DisplayName,
            IsActive = true
        };

        _context.ClientUsers.Add(clientUser);

        await _context.SaveChangesAsync(cancellationToken);

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
            HasPin = false
        };
    }
}
