using FluentValidation;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.");
        RuleFor(x => x.ClientId).NotNull().When(x => x.UserType == UserType.Client)
            .WithMessage("Los usuarios cliente requieren un cliente.");
        RuleFor(x => x.ClientId).Null().When(x => x.UserType == UserType.Administrator)
            .WithMessage("Los usuarios administradores no pueden tener alcance de cliente.");
    }
}
