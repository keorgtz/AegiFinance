using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Services.Queries.GetServiceById;

public class GetServiceByIdQuery : IRequest<ServiceDetailDto>
{
    public Guid Id { get; set; }

    public GetServiceByIdQuery() { }

    public GetServiceByIdQuery(Guid id)
    {
        Id = id;
    }
}
