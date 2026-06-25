using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPermissionService _permissionService;

    public UpdateUserCommandHandler(IApplicationDbContext context, IPermissionService permissionService)
    {
        _context = context;
        _permissionService = permissionService;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("El usuario no existe.");
        }

        var emailTaken = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != request.Id && u.Email == request.Email, cancellationToken);

        if (emailTaken)
        {
            throw new InvalidOperationException("Ya existe un usuario con el mismo correo electrónico.");
        }

        user.Email = request.Email;
        user.Name = request.Name;

        await _context.SaveChangesAsync(cancellationToken);

        var permissions = await _permissionService.GetEffectivePermissionsAsync(user.Id, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            UserType = user.UserType.ToString(),
            ClientId = user.ClientId,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            Roles = user.Roles.Select(r => r.Name).ToList(),
            Permissions = permissions.ToList()
        };
    }
}
