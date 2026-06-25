using FluentValidation;

namespace AegiFinance.Application.Features.Auth.Commands.LoginWithPin;

public class LoginWithPinCommandValidator : AbstractValidator<LoginWithPinCommand>
{
    public LoginWithPinCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.");

        RuleFor(x => x.Pin)
            .NotEmpty().WithMessage("El PIN es obligatorio.")
            .Matches("^[0-9]{4,6}$").WithMessage("El PIN debe ser numérico, de 4 a 6 dígitos.");
    }
}
