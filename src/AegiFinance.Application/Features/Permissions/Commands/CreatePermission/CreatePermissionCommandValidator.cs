using FluentValidation;

namespace AegiFinance.Application.Features.Permissions.Commands.CreatePermission;

public class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("El código del permiso es obligatorio.")
            .MaximumLength(100).WithMessage("El código no puede exceder 100 caracteres.")
            .Matches("^[A-Za-z][A-Za-z0-9]*$").WithMessage("El código debe ser alfanumérico, sin espacios.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del permiso es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres.");
    }
}
