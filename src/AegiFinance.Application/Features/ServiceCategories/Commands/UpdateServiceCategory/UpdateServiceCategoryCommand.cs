using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ServiceCategories.Commands.UpdateServiceCategory;

public class UpdateServiceCategoryCommand : IRequest<ServiceCategoryDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
