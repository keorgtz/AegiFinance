using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Queries.GetServices;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, PaginatedList<ServiceListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<ServiceListDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Services
            .AsNoTracking()
            .Include(s => s.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Code.ToLower().Contains(term));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == request.CategoryId.Value);
        }

        if (request.BillingType.HasValue)
        {
            query = query.Where(s => s.BillingType == request.BillingType.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        query = query.OrderBy(s => s.Name);

        var projected = query.Select(s => new ServiceListDto
        {
            Id = s.Id,
            Code = s.Code,
            Name = s.Name,
            CategoryName = s.Category != null ? s.Category.Name : null,
            BillingType = s.BillingType.ToString(),
            DefaultPrice = s.DefaultPrice,
            Currency = s.Currency,
            IsActive = s.IsActive,
            IsPublic = s.IsPublic
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
