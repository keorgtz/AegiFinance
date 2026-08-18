using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Services.Commands.CreateServiceVersion;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Queries.GetServiceVersions;
public sealed class GetServiceVersionsQueryHandler(IApplicationDbContext context) : IRequestHandler<GetServiceVersionsQuery, IReadOnlyList<ServiceVersionDto>>
{
    public async Task<IReadOnlyList<ServiceVersionDto>> Handle(GetServiceVersionsQuery request, CancellationToken cancellationToken)
        => (await context.ServiceVersions.AsNoTracking().Include(item => item.Concepts).Where(item => item.ServiceId == request.ServiceId)
            .OrderByDescending(item => item.VersionNumber).ToListAsync(cancellationToken)).Select(CreateServiceVersionCommandHandler.Map).ToList();
}
