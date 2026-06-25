using FluentValidation;

namespace AegiFinance.Application.Features.Clients.Commands.UpdateClientContact;

public class UpdateClientContactCommandValidator : AbstractValidator<UpdateClientContactCommand>
{
    public UpdateClientContactCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del contacto es obligatorio.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El correo electrónico no es válido.");
    }
}
