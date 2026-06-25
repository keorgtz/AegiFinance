using FluentValidation;

namespace AegiFinance.Application.Features.SubscriptionPermissions.Commands.CreateSubscriptionPermission;

public class CreateSubscriptionPermissionCommandValidator : AbstractValidator<CreateSubscriptionPermissionCommand>
{
    public CreateSubscriptionPermissionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty).WithMessage("El usuario es obligatorio.");

        RuleFor(x => x.SubscriptionId)
            .NotEqual(Guid.Empty).WithMessage("La suscripción es obligatoria.");
    }
}
