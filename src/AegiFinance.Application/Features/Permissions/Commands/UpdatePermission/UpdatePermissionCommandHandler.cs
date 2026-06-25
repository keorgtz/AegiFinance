using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Permissions.Commands.UpdatePermission;

public class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, PermissionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdatePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionDto> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (permission is null)
        {
            throw new InvalidOperationException("El permiso no existe.");
        }

        permission.Name = request.Name;
        permission.Description = request.Description;

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
