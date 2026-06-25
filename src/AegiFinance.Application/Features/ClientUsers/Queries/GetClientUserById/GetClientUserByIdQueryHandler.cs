using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ClientUsers.Queries.GetClientUserById;

public class GetClientUserByIdQueryHandler : IRequestHandler<GetClientUserByIdQuery, ClientUserDto?>
{
    private readonly IApplicationDbContext _context;

    public GetClientUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClientUserDto?> Handle(GetClientUserByIdQuery request, CancellationToken cancellationToken)
    {
        var clientUser = await _context.ClientUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(cu => cu.Id == request.Id, cancellationToken);

        if (clientUser is null)
        {
            return null;
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == clientUser.UserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var hasPin = await _context.ClientPinCredentials
            .AsNoTracking()
            .AnyAsync(p => p.UserId == user.Id, cancellationToken);

        return new ClientUserDto
        {
            Id = clientUser.Id,
            ClientId = clientUser.ClientId,
            UserId = user.Id,
            DisplayName = clientUser.DisplayName,
            IsActive = clientUser.IsActive,
            UserName = user.UserName,
            Email = user.Email,
            Name = user.Name,
            HasPin = hasPin
        };
    }
}
