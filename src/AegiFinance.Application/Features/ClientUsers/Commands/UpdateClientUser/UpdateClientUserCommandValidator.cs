using FluentValidation;

namespace AegiFinance.Application.Features.ClientUsers.Commands.UpdateClientUser;

public class UpdateClientUserCommandValidator : AbstractValidator<UpdateClientUserCommand>
{
    public UpdateClientUserCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("El nombre para mostrar es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre para mostrar no puede exceder 150 caracteres.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .MaximumLength(200).WithMessage("El correo electrónico no puede exceder 200 caracteres.");
    }
}
