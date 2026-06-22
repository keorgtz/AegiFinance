using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ServiceCategories.Commands.CreateServiceCategory;

public class CreateServiceCategoryCommand : IRequest<ServiceCategoryDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
