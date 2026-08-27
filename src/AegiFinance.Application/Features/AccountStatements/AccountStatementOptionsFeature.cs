using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.AccountStatements;

public sealed record AccountStatementClientOptionDto(Guid Id, string Code, string Name);
public sealed record AccountStatementSubscriptionOptionDto(Guid Id, string Code, string ServiceName);
public sealed record GetAccountStatementClientsQuery : IRequest<IReadOnlyList<AccountStatementClientOptionDto>>;
public sealed record GetAccountStatementSubscriptionsQuery(Guid ClientId) : IRequest<IReadOnlyList<AccountStatementSubscriptionOptionDto>>;

public sealed class GetAccountStatementClientsQueryHandler : IRequestHandler<GetAccountStatementClientsQuery, IReadOnlyList<AccountStatementClientOptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetAccountStatementClientsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task<IReadOnlyList<AccountStatementClientOptionDto>> Handle(GetAccountStatementClientsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Clients.AsNoTracking();
        if (_currentUser.IsClientUser())
        {
            var clientId = _currentUser.ClientId ?? throw new UnauthorizedAccessException("No hay un cliente asociado al usuario.");
            query = query.Where(item => item.Id == clientId);
        }
        return await query.OrderBy(item => item.Name).Select(item => new AccountStatementClientOptionDto(item.Id, item.Code, item.Name)).ToListAsync(cancellationToken);
    }
}

public sealed class GetAccountStatementSubscriptionsQueryHandler : IRequestHandler<GetAccountStatementSubscriptionsQuery, IReadOnlyList<AccountStatementSubscriptionOptionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetAccountStatementSubscriptionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task<IReadOnlyList<AccountStatementSubscriptionOptionDto>> Handle(GetAccountStatementSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        GetAccountStatementInquiriesQueryHandler.EnsureClientScope(_currentUser, request.ClientId);
        var query = _context.Subscriptions.AsNoTracking().Where(item => item.ClientId == request.ClientId);
        if (_currentUser.IsClientUser() && _currentUser.UserId.HasValue)
        {
            var allowed = await GetAccountStatementInquiriesQueryHandler.RestrictedSubscriptionsAsync(_context, _currentUser.UserId.Value, cancellationToken);
            if (allowed.Count > 0) query = query.Where(item => allowed.Contains(item.Id));
        }
        return await query.OrderBy(item => item.Code)
            .Select(item => new AccountStatementSubscriptionOptionDto(item.Id, item.Code, item.Service.Name)).ToListAsync(cancellationToken);
    }
}
