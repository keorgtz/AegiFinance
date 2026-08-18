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
        RuleFor(x => x.PresentationCurrency).NotEmpty().Length(3);
        RuleFor(x => x.PaymentTermsDays).InclusiveBetween(0, 365);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommercialTerms).MaximumLength(2000);
    }
}
