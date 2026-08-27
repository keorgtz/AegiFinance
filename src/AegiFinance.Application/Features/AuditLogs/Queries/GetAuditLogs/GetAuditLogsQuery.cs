using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQuery : IRequest<PaginatedList<AuditLogDto>>
{
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
