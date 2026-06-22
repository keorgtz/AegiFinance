using FluentValidation;

namespace AegiFinance.Application.Features.Clients.Commands.AddClientNote;

public class AddClientNoteCommandValidator : AbstractValidator<AddClientNoteCommand>
{
    public AddClientNoteCommandValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("El cliente es obligatorio.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido de la nota es obligatorio.");
    }
}
