using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            throw new InvalidOperationException("El rol no existe.");
        }

        var nameTaken = await _context.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id != request.Id && r.Name == request.Name, cancellationToken);

        if (nameTaken)
        {
            throw new InvalidOperationException("Ya existe un rol con el mismo nombre.");
        }

        role.Name = request.Name;
        role.Description = request.Description;
        role.UserType = request.UserType;

        await _context.SaveChangesAsync(cancellationToken);

        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            UserType = role.UserType?.ToString()
        };
    }
}
