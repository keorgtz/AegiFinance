using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Queries.GetClientUsers;

public class GetClientUsersQueryHandler : IRequestHandler<GetClientUsersQuery, List<ClientUserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClientUsersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ClientUserDto>> Handle(GetClientUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ClientUsers.AsNoTracking().AsQueryable();

        var isClientCaller = Enum.TryParse<UserType>(_currentUserService.UserType, out var callerType)
            && callerType == UserType.Client;

        if (isClientCaller)
        {
            if (!_currentUserService.ClientId.HasValue)
            {
                return new List<ClientUserDto>();
            }

            query = query.Where(cu => cu.ClientId == _currentUserService.ClientId.Value);
        }
        else if (request.ClientId.HasValue)
        {
            query = query.Where(cu => cu.ClientId == request.ClientId.Value);
        }

        var clientUserIds = await query.Select(cu => cu.UserId).ToListAsync(cancellationToken);

        var pinUserIds = (await _context.ClientPinCredentials
            .AsNoTracking()
            .Where(p => clientUserIds.Contains(p.UserId))
            .Select(p => p.UserId)
            .ToListAsync(cancellationToken)).ToHashSet();

        return await query
            .OrderBy(cu => cu.DisplayName)
            .Join(_context.Users.AsNoTracking(), cu => cu.UserId, u => u.Id, (cu, u) => new ClientUserDto
            {
                Id = cu.Id,
                ClientId = cu.ClientId,
                UserId = u.Id,
                DisplayName = cu.DisplayName,
                IsActive = cu.IsActive,
                UserName = u.UserName,
                Email = u.Email,
                Name = u.Name,
                HasPin = pinUserIds.Contains(u.Id)
            })
            .ToListAsync(cancellationToken);
    }
}
