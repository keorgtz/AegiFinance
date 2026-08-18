using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserName == request.UserName || u.Email == request.Email, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe un usuario con el mismo nombre de usuario o correo electrónico.");
        }

        var organizationId = _currentUser.OrganizationId ?? throw new InvalidOperationException("El usuario no tiene una organización asignada.");
        if (request.ClientId.HasValue && !await _context.Clients.AsNoTracking().AnyAsync(client => client.Id == request.ClientId && client.OrganizationId == organizationId, cancellationToken))
            throw new InvalidOperationException("El cliente no pertenece a la organización.");
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Name = request.Name,
            UserType = request.UserType,
            ClientId = request.ClientId,
            OrganizationId = organizationId,
            IsActive = true,
            EmailConfirmed = false,
            MustChangePassword = true,
            PasswordChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            UserType = user.UserType.ToString(),
            ClientId = user.ClientId,
            OrganizationId = user.OrganizationId,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed
        };
    }
}
