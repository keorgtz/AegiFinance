using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Queries.GetClients;

public class GetClientsQueryHandler : IRequestHandler<GetClientsQuery, PaginatedList<ClientListDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClientsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<ClientListDto>> Handle(GetClientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clients
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .Include(c => c.Contacts)
            .AsQueryable();

        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser)
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                return new PaginatedList<ClientListDto>(new List<ClientListDto>(), 0, request.PageNumber, request.PageSize);
            }

            query = query.Where(c => c.Id == _currentUserService.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Code.ToLower().Contains(term) ||
                (c.TradeName != null && c.TradeName.ToLower().Contains(term)));
        }

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(c => c.CategoryId == request.CategoryId.Value);
        }

        if (request.TagId.HasValue)
        {
            query = query.Where(c => c.Tags.Any(t => t.Id == request.TagId.Value));
        }

        query = query.OrderBy(c => c.Name);

        var projected = query.Select(c => new ClientListDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            TradeName = c.TradeName,
            TaxId = c.TaxId,
            Status = c.Status.ToString(),
            CategoryName = c.Category != null ? c.Category.Name : null,
            PrimaryContactName = c.Contacts.Where(con => con.IsPrimary).Select(con => con.Name).FirstOrDefault(),
            Tags = c.Tags.Select(t => new ClientTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            }).ToList()
        });

        return await projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
