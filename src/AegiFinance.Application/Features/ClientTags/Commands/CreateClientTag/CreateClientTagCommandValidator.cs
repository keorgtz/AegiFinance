using FluentValidation;

namespace AegiFinance.Application.Features.ClientTags.Commands.CreateClientTag;

public class CreateClientTagCommandValidator : AbstractValidator<CreateClientTagCommand>
{
    public CreateClientTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la etiqueta es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de la etiqueta no puede exceder 100 caracteres.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color de la etiqueta es obligatorio.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("El color debe tener formato hexadecimal, por ejemplo #6548EB.");
    }
}
