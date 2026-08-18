using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking().Include(u => u.Roles).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(u =>
                u.UserName.Contains(term) ||
                u.Email.Contains(term) ||
                u.Name.Contains(term));
        }

        if (request.UserType.HasValue)
        {
            query = query.Where(u => u.UserType == request.UserType.Value);
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(u => u.ClientId == request.ClientId.Value);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == request.IsActive.Value);
        }

        var projected = query
            .OrderBy(u => u.Name)
            .Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                Name = u.Name,
                UserType = u.UserType.ToString(),
                ClientId = u.ClientId,
                IsActive = u.IsActive,
                EmailConfirmed = u.EmailConfirmed,
                LastLoginAt = u.LastLoginAt,
                LockoutEnd = u.LockoutEnd,
                MustChangePassword = u.MustChangePassword,
                ActiveSessionCount = _context.UserSessions.Count(session => session.UserId == u.Id && session.RevokedAt == null && session.ExpiresAt > DateTime.UtcNow),
                Roles = u.Roles.Select(r => r.Name).ToList()
            });

        return projected.ToPaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
