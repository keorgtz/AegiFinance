using FluentValidation;

namespace AegiFinance.Application.Features.ClientUsers.Commands.SetClientUserPin;

public class SetClientUserPinCommandValidator : AbstractValidator<SetClientUserPinCommand>
{
    public SetClientUserPinCommandValidator()
    {
        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("El PIN es obligatorio.")
            .Matches("^[0-9]{4,6}$").WithMessage("El PIN debe tener entre 4 y 6 dígitos numéricos.");
    }
}
