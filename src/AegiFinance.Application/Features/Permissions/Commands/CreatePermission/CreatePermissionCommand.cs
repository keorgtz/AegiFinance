using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Permissions.Commands.CreatePermission;

public class CreatePermissionCommand : IRequest<PermissionDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
