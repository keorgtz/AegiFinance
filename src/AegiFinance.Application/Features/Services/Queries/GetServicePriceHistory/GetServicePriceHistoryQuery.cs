using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Services.Queries.GetServicePriceHistory;

public class GetServicePriceHistoryQuery : IRequest<List<ServicePriceHistoryDto>>
{
    public Guid ServiceId { get; set; }

    public GetServicePriceHistoryQuery() { }

    public GetServicePriceHistoryQuery(Guid serviceId)
    {
        ServiceId = serviceId;
    }
}
