using FluentValidation;

namespace AegiFinance.Application.Features.ClientCategories.Commands.UpdateClientCategory;

public class UpdateClientCategoryCommandValidator : AbstractValidator<UpdateClientCategoryCommand>
{
    public UpdateClientCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador de la categoría es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la categoría no puede exceder 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}
