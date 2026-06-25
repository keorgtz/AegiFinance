using AegiFinance.Application.Common.Validators;
using FluentValidation;

namespace AegiFinance.Application.Features.ClientUsers.Commands.CreateClientUser;

public class CreateClientUserCommandValidator : AbstractValidator<CreateClientUserCommand>
{
    public CreateClientUserCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEqual(Guid.Empty).WithMessage("El cliente es obligatorio.");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre de usuario no puede exceder 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(200).WithMessage("El correo electrónico no puede exceder 200 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MustBeStrongPassword();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("El nombre para mostrar es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre para mostrar no puede exceder 150 caracteres.");
    }
}
