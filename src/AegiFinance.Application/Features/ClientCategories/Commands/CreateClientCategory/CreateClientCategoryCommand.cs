using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.ClientCategories.Commands.CreateClientCategory;

public class CreateClientCategoryCommand : IRequest<ClientCategoryDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
