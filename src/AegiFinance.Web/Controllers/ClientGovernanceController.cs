using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/client-governance")]
[Authorize]
public sealed class ClientGovernanceController : ControllerBase
{
    private readonly IClientGovernanceService _governance;
    public ClientGovernanceController(IClientGovernanceService governance) => _governance = governance;

    [HttpGet("clients/{clientId:guid}/timeline")]
    [Authorize(Policy = "ViewClientTimeline")]
    public async Task<ActionResult<IReadOnlyList<ClientTimelineItemDto>>> Timeline(Guid clientId, CancellationToken cancellationToken)
        => Ok(await _governance.GetTimelineAsync(clientId, cancellationToken));

    [HttpGet("duplicate-rule")]
    [Authorize(Policy = "ManageClientDuplicateRules")]
    public async Task<ActionResult<ClientDuplicateRuleDto>> GetRule(CancellationToken cancellationToken)
        => Ok(await _governance.GetDuplicateRuleAsync(cancellationToken));

    [HttpPut("duplicate-rule")]
    [Authorize(Policy = "ManageClientDuplicateRules")]
    public async Task<ActionResult<ClientDuplicateRuleDto>> UpdateRule(ClientDuplicateRuleDto rule, CancellationToken cancellationToken)
        => Ok(await _governance.UpdateDuplicateRuleAsync(rule, cancellationToken));

    [HttpGet("duplicates")]
    [Authorize(Policy = "ViewClients")]
    public async Task<ActionResult<IReadOnlyList<ClientDuplicateMatchDto>>> Duplicates([FromQuery] string name, [FromQuery] string? taxId, [FromQuery] string? billingEmail, [FromQuery] Guid? excludeClientId, CancellationToken cancellationToken)
        => Ok(await _governance.FindDuplicatesAsync(name, taxId, billingEmail, excludeClientId, cancellationToken));
}
