using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankImports;

public sealed class CreateBankImportProfileCommand : IRequest<BankImportProfileDto>
{
    public string Name { get; set; } = string.Empty;
    public string AdapterCode { get; set; } = "generic";
    public string DateColumn { get; set; } = string.Empty;
    public string DescriptionColumn { get; set; } = string.Empty;
    public string? ReferenceColumn { get; set; }
    public string? AmountColumn { get; set; }
    public string? DebitColumn { get; set; }
    public string? CreditColumn { get; set; }
    public string? CurrencyColumn { get; set; }
    public string? BalanceColumn { get; set; }
    public string? DateFormat { get; set; }
    public string Delimiter { get; set; } = ",";
    public int HeaderRow { get; set; } = 1;
}

public sealed class CreateBankImportProfileCommandValidator : AbstractValidator<CreateBankImportProfileCommand>
{
    public CreateBankImportProfileCommandValidator()
    {
        RuleFor(item => item.Name).NotEmpty().MaximumLength(120);
        RuleFor(item => item.AdapterCode).Must(code => BankImportMapping.Adapters.Any(item => item.Code == code)).WithMessage("El adaptador no es válido.");
        RuleFor(item => item.DateColumn).NotEmpty().MaximumLength(120);
        RuleFor(item => item.DescriptionColumn).NotEmpty().MaximumLength(120);
        RuleFor(item => item).Must(item => !string.IsNullOrWhiteSpace(item.AmountColumn) || !string.IsNullOrWhiteSpace(item.DebitColumn) || !string.IsNullOrWhiteSpace(item.CreditColumn))
            .WithMessage("Definí una columna de monto o columnas de cargo/abono.");
        RuleFor(item => item.Delimiter).NotEmpty().MaximumLength(4);
        RuleFor(item => item.HeaderRow).InclusiveBetween(1, 100);
    }
}

public sealed class CreateBankImportProfileCommandHandler : IRequestHandler<CreateBankImportProfileCommand, BankImportProfileDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateBankImportProfileCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task<BankImportProfileDto> Handle(CreateBankImportProfileCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        if (await _context.BankImportProfiles.AnyAsync(item => item.Name == request.Name.Trim(), cancellationToken))
            throw new InvalidOperationException("Ya existe un perfil con ese nombre.");
        var profile = new BankImportProfile
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, Name = request.Name.Trim(), AdapterCode = request.AdapterCode,
            DateColumn = request.DateColumn.Trim(), DescriptionColumn = request.DescriptionColumn.Trim(), ReferenceColumn = Clean(request.ReferenceColumn),
            AmountColumn = Clean(request.AmountColumn), DebitColumn = Clean(request.DebitColumn), CreditColumn = Clean(request.CreditColumn),
            CurrencyColumn = Clean(request.CurrencyColumn), BalanceColumn = Clean(request.BalanceColumn), DateFormat = Clean(request.DateFormat),
            Delimiter = request.Delimiter, HeaderRow = request.HeaderRow, IsActive = true
        };
        _context.BankImportProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);
        return new BankImportProfileDto
        {
            Id = profile.Id, Name = profile.Name, AdapterCode = profile.AdapterCode, DateColumn = profile.DateColumn,
            DescriptionColumn = profile.DescriptionColumn, ReferenceColumn = profile.ReferenceColumn, AmountColumn = profile.AmountColumn,
            DebitColumn = profile.DebitColumn, CreditColumn = profile.CreditColumn, CurrencyColumn = profile.CurrencyColumn,
            BalanceColumn = profile.BalanceColumn, DateFormat = profile.DateFormat, Delimiter = profile.Delimiter, HeaderRow = profile.HeaderRow
        };
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
