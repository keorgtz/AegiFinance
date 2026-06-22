using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientCategories.Queries.GetClientCategories;

public class GetClientCategoriesQueryHandler : IRequestHandler<GetClientCategoriesQuery, List<ClientCategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientCategoryDto>> Handle(GetClientCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.ClientCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ClientCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            })
            .ToListAsync(cancellationToken);
    }
}
