using FluentValidation;

namespace AegiFinance.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("El identificador del cliente es obligatorio.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.");

        RuleFor(x => x.BillingEmail)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.BillingEmail))
            .WithMessage("El correo de facturación no es válido.");
    }
}
