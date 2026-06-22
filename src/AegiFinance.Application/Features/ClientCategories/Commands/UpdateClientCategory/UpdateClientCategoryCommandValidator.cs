using FluentValidation;

namespace AegiFinance.Application.Features.ClientCategories.Commands.UpdateClientCategory;

public class UpdateClientCategoryCommandValidator : AbstractValidator<UpdateClientCategoryCommand>
{
    public UpdateClientCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador de la categoría es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.");
    }
}
