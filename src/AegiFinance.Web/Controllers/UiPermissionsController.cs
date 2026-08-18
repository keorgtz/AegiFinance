using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/ui-permissions")]
[Authorize]
public sealed class UiPermissionsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IUiPermissionService _uiPermissions;

    public UiPermissionsController(IApplicationDbContext context, ICurrentUserService currentUser, IUiPermissionService uiPermissions)
    {
        _context = context;
        _currentUser = currentUser;
        _uiPermissions = uiPermissions;
    }

    [HttpGet("catalog")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> GetCatalog(CancellationToken cancellationToken) =>
        Ok(await _context.UiControlDefinitions.AsNoTracking().OrderBy(item => item.Module).ThenBy(item => item.Label).ToListAsync(cancellationToken));

    [HttpPost("catalog/sync")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> SyncCatalog(List<UiControlDefinitionDto> controls, CancellationToken cancellationToken)
    {
        await _uiPermissions.SyncCatalogAsync(controls, cancellationToken);
        return NoContent();
    }

    [HttpGet("effective")]
    public async Task<IActionResult> GetEffective([FromQuery] Guid? clientId, [FromQuery] Guid? subscriptionId, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId) return Unauthorized();
        return Ok(await _uiPermissions.GetEffectivePoliciesAsync(userId, clientId, subscriptionId, cancellationToken));
    }

    [HttpPut("policy")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> SetPolicy(UiControlPolicyDto policy, CancellationToken cancellationToken)
    {
        await _uiPermissions.SetPolicyAsync(policy, cancellationToken);
        return NoContent();
    }

    [HttpPost("simulate")]
    [Authorize(Policy = "ManageRoles")]
    public async Task<IActionResult> Simulate(UiPermissionSimulationRequest request, CancellationToken cancellationToken) =>
        Ok(await _uiPermissions.SimulateAsync(request.RoleId, request.UserId, request.ClientId, request.SubscriptionId, cancellationToken));
}
