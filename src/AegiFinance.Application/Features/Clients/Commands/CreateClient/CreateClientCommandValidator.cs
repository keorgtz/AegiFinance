using FluentValidation;

namespace AegiFinance.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre del cliente no puede exceder 200 caracteres.");

        RuleFor(x => x.TradeName).MaximumLength(200);
        RuleFor(x => x.TaxId).MaximumLength(50);

        RuleFor(x => x.BillingEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.BillingEmail))
            .WithMessage("El correo de facturación no es válido.")
            .MaximumLength(256);
        RuleFor(x => x.BillingAddress).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.PresentationCurrency).NotEmpty().Length(3);
        RuleFor(x => x.PaymentTermsDays).InclusiveBetween(0, 365);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CommercialTerms).MaximumLength(2000);
    }
}
