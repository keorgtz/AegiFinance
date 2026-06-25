using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.AuditLogs.Queries.GetAuditLogs;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedList<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            query = query.Where(a => a.EntityType == request.EntityType);
        }

        if (request.UserId.HasValue)
        {
            query = query.Where(a => a.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(a => a.Action == request.Action);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(a => a.Timestamp >= request.FromDate.Value);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(a => a.Timestamp <= request.ToDate.Value);
        }

        var projected = query
            .OrderByDescending(a => a.Timestamp)
            .GroupJoin(_context.Users.AsNoTracking(), a => a.UserId, u => u.Id, (a, users) => new { a, users })
            .SelectMany(x => x.users.DefaultIfEmpty(), (x, u) => new AuditLogDto
            {
                Id = x.a.Id,
                EntityType = x.a.EntityType,
                EntityId = x.a.EntityId,
                Action = x.a.Action,
                Changes = x.a.Changes,
                UserId = x.a.UserId,
                UserName = u != null ? u.Name : null,
                Timestamp = x.a.Timestamp,
                IPAddress = x.a.IPAddress,
                UserAgent = x.a.UserAgent
            });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
