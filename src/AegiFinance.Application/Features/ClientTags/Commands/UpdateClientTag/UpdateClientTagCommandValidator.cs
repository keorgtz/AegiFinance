using FluentValidation;

namespace AegiFinance.Application.Features.ClientTags.Commands.UpdateClientTag;

public class UpdateClientTagCommandValidator : AbstractValidator<UpdateClientTagCommand>
{
    public UpdateClientTagCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador de la etiqueta es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre de la etiqueta es obligatorio.");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("El color de la etiqueta es obligatorio.");
    }
}
