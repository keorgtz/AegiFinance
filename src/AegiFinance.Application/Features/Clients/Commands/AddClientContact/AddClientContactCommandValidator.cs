using FluentValidation;

namespace AegiFinance.Application.Features.Clients.Commands.AddClientContact;

public class AddClientContactCommandValidator : AbstractValidator<AddClientContactCommand>
{
    public AddClientContactCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("El cliente es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del contacto es obligatorio.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("El correo electrónico no es válido.");
    }
}
