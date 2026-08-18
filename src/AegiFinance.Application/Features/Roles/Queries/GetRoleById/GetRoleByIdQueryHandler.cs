using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetRoleByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RoleDetailDto?> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .AsNoTracking()
            .Include(r => r.RolePermissions)
                .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (role is null)
        {
            return null;
        }

        return new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            UserType = role.UserType?.ToString(),
            Permissions = role.RolePermissions.Select(rolePermission => new PermissionDto
            {
                Id = rolePermission.Permission.Id,
                Code = rolePermission.Permission.Code,
                Name = rolePermission.Permission.Name,
                Description = rolePermission.Permission.Description,
                Module = rolePermission.Permission.Module,
                Action = rolePermission.Permission.Action,
                Kind = rolePermission.Permission.Kind.ToString(),
                IsSystemGenerated = rolePermission.Permission.IsSystemGenerated,
                IsActive = rolePermission.Permission.IsActive
            }).ToList()
        };
    }
}
