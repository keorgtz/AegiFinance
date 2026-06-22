using FluentValidation;

namespace AegiFinance.Application.Features.ClientCategories.Commands.CreateClientCategory;

public class CreateClientCategoryCommandValidator : AbstractValidator<CreateClientCategoryCommand>
{
    public CreateClientCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.");
    }
}
