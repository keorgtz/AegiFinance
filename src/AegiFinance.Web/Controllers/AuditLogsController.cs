using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AuditLogs.Queries.GetAuditLogs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "ViewAccountingAudit")]
    public async Task<ActionResult<PaginatedList<AuditLogDto>>> GetAll([FromQuery] GetAuditLogsQuery query, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(query, cancellationToken));
    }
}
