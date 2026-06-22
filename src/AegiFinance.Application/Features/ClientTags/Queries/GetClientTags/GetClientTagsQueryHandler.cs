using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientTags.Queries.GetClientTags;

public class GetClientTagsQueryHandler : IRequestHandler<GetClientTagsQuery, List<ClientTagDto>>
{
    private readonly IApplicationDbContext _context;

    public GetClientTagsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientTagDto>> Handle(GetClientTagsQuery request, CancellationToken cancellationToken)
    {
        return await _context.ClientTags
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new ClientTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            })
            .ToListAsync(cancellationToken);
    }
}
