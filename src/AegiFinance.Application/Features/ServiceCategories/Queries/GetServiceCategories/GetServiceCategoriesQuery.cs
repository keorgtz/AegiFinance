using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ServiceCategories.Queries.GetServiceCategories;

public class GetServiceCategoriesQuery : IRequest<List<ServiceCategoryDto>>
{
}
