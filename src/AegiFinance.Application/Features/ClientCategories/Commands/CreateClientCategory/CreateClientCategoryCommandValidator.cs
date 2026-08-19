using FluentValidation;

namespace AegiFinance.Application.Features.ClientCategories.Commands.CreateClientCategory;

public class CreateClientCategoryCommandValidator : AbstractValidator<CreateClientCategoryCommand>
{
    public CreateClientCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la categoría no puede exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}
