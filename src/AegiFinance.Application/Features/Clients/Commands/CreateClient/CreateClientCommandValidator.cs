using FluentValidation;

namespace AegiFinance.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.");

        RuleFor(x => x.BillingEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.BillingEmail))
            .WithMessage("El correo de facturación no es válido.");
    }
}
