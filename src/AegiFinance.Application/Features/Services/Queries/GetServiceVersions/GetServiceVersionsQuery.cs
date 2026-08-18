using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Services.Queries.GetServiceVersions;
public sealed record GetServiceVersionsQuery(Guid ServiceId) : IRequest<IReadOnlyList<ServiceVersionDto>>;
