using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AegiFinance.Domain.Accounting;

namespace AegiFinance.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClientCodeGenerator _codeGenerator;
    private readonly ICurrentUserService _currentUserService;

    public CreateClientCommandHandler(IApplicationDbContext context, IClientCodeGenerator codeGenerator, ICurrentUserService currentUserService)
    {
        _context = context;
        _codeGenerator = codeGenerator;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para crear clientes.");
        }
        var organizationId = _currentUserService.OrganizationId
            ?? throw new InvalidOperationException("El usuario no tiene una organización asignada.");
        var normalizedName = ClientIdentityRules.Normalize(request.Name);
        var normalizedTaxId = ClientIdentityRules.NormalizeOptional(request.TaxId);
        var normalizedEmail = ClientIdentityRules.NormalizeOptional(request.BillingEmail);
        var duplicateRule = await _context.ClientDuplicateRules.AsNoTracking()
            .FirstOrDefaultAsync(rule => rule.OrganizationId == organizationId, cancellationToken);
        var matchTax = duplicateRule?.MatchTaxId ?? true;
        var matchName = duplicateRule?.MatchName ?? true;
        var matchEmail = duplicateRule?.MatchBillingEmail ?? true;
        var duplicate = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(client =>
            client.OrganizationId == organizationId &&
            ((matchTax && normalizedTaxId != null && client.NormalizedTaxId == normalizedTaxId) ||
             (matchName && client.NormalizedName == normalizedName) ||
             (matchEmail && normalizedEmail != null && client.NormalizedBillingEmail == normalizedEmail)), cancellationToken);
        if (duplicate is not null && (duplicateRule?.BlockOnMatch ?? true))
            throw new InvalidOperationException($"Posible cliente duplicado: {duplicate.Code} · {duplicate.Name}.");

        if (!await _context.CurrencyConfigs.AsNoTracking().AnyAsync(currency => currency.Code == request.PresentationCurrency && currency.IsActive, cancellationToken))
            throw new InvalidOperationException("La moneda de presentación no está activa.");
        if (request.AccountManagerUserId.HasValue && !await _context.Users.AsNoTracking().AnyAsync(user =>
                user.Id == request.AccountManagerUserId && user.OrganizationId == organizationId && user.IsActive, cancellationToken))
            throw new InvalidOperationException("El responsable seleccionado no pertenece a la organización.");

        ClientCategory? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await _context.ClientCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (category is null)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }
        }

        List<ClientTag> tags = new();
        if (request.TagIds?.Count > 0)
        {
            tags = await _context.ClientTags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (tags.Count != request.TagIds.Distinct().Count())
            {
                throw new InvalidOperationException("Una o más etiquetas no existen.");
            }
        }

        var code = await _codeGenerator.GenerateAsync(cancellationToken);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Code = code,
            Name = request.Name,
            TradeName = request.TradeName,
            TaxId = request.TaxId,
            BillingEmail = request.BillingEmail,
            BillingAddress = request.BillingAddress,
            Phone = request.Phone,
            Status = request.Status,
            Notes = request.Notes,
            CategoryId = request.CategoryId,
            Category = category,
            Tags = tags,
            PresentationCurrency = request.PresentationCurrency.ToUpperInvariant(),
            PaymentTermsDays = request.PaymentTermsDays,
            CreditLimit = request.CreditLimit,
            CommercialTerms = request.CommercialTerms,
            AccountManagerUserId = request.AccountManagerUserId,
            NormalizedName = normalizedName,
            NormalizedTaxId = normalizedTaxId,
            NormalizedBillingEmail = normalizedEmail
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(client);
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            Code = client.Code,
            Name = client.Name,
            TradeName = client.TradeName,
            TaxId = client.TaxId,
            BillingEmail = client.BillingEmail,
            BillingAddress = client.BillingAddress,
            Phone = client.Phone,
            Status = client.Status.ToString(),
            Notes = client.Notes,
            CategoryId = client.CategoryId,
            CategoryName = client.Category?.Name,
            Tags = client.Tags.Select(t => new ClientTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            }).ToList(),
            PresentationCurrency = client.PresentationCurrency,
            PaymentTermsDays = client.PaymentTermsDays,
            CreditLimit = client.CreditLimit,
            CommercialTerms = client.CommercialTerms,
            AccountManagerUserId = client.AccountManagerUserId,
            AccountManagerName = client.AccountManagerUser?.Name
        };
    }
}
