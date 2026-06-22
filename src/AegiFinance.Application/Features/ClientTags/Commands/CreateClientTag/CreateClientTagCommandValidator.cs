using FluentValidation;

namespace AegiFinance.Application.Features.ClientTags.Commands.CreateClientTag;

public class CreateClientTagCommandValidator : AbstractValidator<CreateClientTagCommand>
{
    public CreateClientTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la etiqueta es obligatorio.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color de la etiqueta es obligatorio.");
    }
}
