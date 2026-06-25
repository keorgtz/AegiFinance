using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Permissions.Commands.CreatePermission;

public class CreatePermissionCommandHandler : IRequestHandler<CreatePermissionCommand, PermissionDto>
{
    private readonly IApplicationDbContext _context;

    public CreatePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionDto> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var exists = await _context.Permissions
            .AsNoTracking()
            .AnyAsync(p => p.Code == request.Code, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Ya existe un permiso con el mismo código.");
        }

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Description = request.Description
        };

        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Name = permission.Name,
            Description = permission.Description
        };
    }
}
