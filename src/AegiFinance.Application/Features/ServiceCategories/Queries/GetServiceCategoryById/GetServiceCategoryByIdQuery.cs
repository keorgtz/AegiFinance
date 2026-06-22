using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ServiceCategories.Queries.GetServiceCategoryById;

public class GetServiceCategoryByIdQuery : IRequest<ServiceCategoryDto>
{
    public Guid Id { get; set; }

    public GetServiceCategoryByIdQuery() { }

    public GetServiceCategoryByIdQuery(Guid id)
    {
        Id = id;
    }
}
