using MediatR;

namespace AegiFinance.Application.Features.ServiceCategories.Commands.DeleteServiceCategory;

public class DeleteServiceCategoryCommand : IRequest
{
    public Guid Id { get; set; }

    public DeleteServiceCategoryCommand() { }

    public DeleteServiceCategoryCommand(Guid id)
    {
        Id = id;
    }
}
