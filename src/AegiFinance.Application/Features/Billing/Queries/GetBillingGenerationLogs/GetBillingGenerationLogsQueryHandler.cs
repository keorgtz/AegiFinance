using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Billing.Queries.GetBillingGenerationLogs;

public class GetBillingGenerationLogsQueryHandler : IRequestHandler<GetBillingGenerationLogsQuery, List<BillingGenerationLogDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBillingGenerationLogsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<BillingGenerationLogDto>> Handle(GetBillingGenerationLogsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar los logs de generación.");
        }

        var query = _context.BillingGenerationLogs
            .AsNoTracking()
            .Include(log => log.BillingCycle)
            .AsQueryable();

        if (request.BillingCycleId.HasValue)
        {
            query = query.Where(log => log.BillingCycleId == request.BillingCycleId.Value);
        }

        if (request.Year.HasValue)
        {
            query = query.Where(log => log.BillingCycle != null && log.BillingCycle.Year == request.Year.Value);
        }

        if (request.Month.HasValue)
        {
            query = query.Where(log => log.BillingCycle != null && log.BillingCycle.Month == request.Month.Value);
        }

        return await query
            .OrderByDescending(log => log.StartedAt)
            .Take(request.Limit)
            .Select(log => new BillingGenerationLogDto
            {
                Id = log.Id,
                BillingCycleId = log.BillingCycleId,
                BillingCycleLabel = log.BillingCycle != null
                    ? $"{log.BillingCycle.Year}-{(log.BillingCycle.Month.HasValue ? log.BillingCycle.Month.Value.ToString("D2") : "**")}"
                    : null,
                StartedAt = log.StartedAt,
                FinishedAt = log.FinishedAt,
                Status = log.Status,
                ItemsGenerated = log.ItemsGenerated,
                Errors = log.Errors,
                TriggeredBy = log.TriggeredBy
            })
            .ToListAsync(cancellationToken);
    }
}
