using AegiFinance.Application.Common.Validators;
using AegiFinance.Domain.Enums;
using FluentValidation;

namespace AegiFinance.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MustBeStrongPassword();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.");

        RuleFor(x => x.ClientId)
            .NotNull().WithMessage("Los usuarios de tipo Client requieren un ClientId.")
            .When(x => x.UserType == UserType.Client);

        RuleFor(x => x.ClientId)
            .Null().WithMessage("Los usuarios de tipo Administrator no deben tener ClientId.")
            .When(x => x.UserType == UserType.Administrator);
    }
}
