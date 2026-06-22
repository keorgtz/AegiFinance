using MediatR;

namespace AegiFinance.Application.Features.ClientCategories.Commands.DeleteClientCategory;

public class DeleteClientCategoryCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteClientCategoryCommand() { }

    public DeleteClientCategoryCommand(Guid id)
    {
        Id = id;
    }
}
