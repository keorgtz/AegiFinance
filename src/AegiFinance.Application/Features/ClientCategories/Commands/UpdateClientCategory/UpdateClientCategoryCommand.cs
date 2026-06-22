using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientCategories.Commands.UpdateClientCategory;

public class UpdateClientCategoryCommand : IRequest<ClientCategoryDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
