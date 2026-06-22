using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientCategories.Queries.GetClientCategories;

public class GetClientCategoriesQuery : IRequest<List<ClientCategoryDto>>
{
}
