using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AegiFinance.Domain.Accounting;

namespace AegiFinance.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClientCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para modificar clientes.");
        }

        var client = await _context.Clients
            .Include(c => c.Tags)
            .Include(c => c.Category)
            .Include(c => c.AccountManagerUser)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }
        var normalizedName = ClientIdentityRules.Normalize(request.Name);
        var normalizedTaxId = ClientIdentityRules.NormalizeOptional(request.TaxId);
        var normalizedEmail = ClientIdentityRules.NormalizeOptional(request.BillingEmail);
        var duplicateRule = await _context.ClientDuplicateRules.AsNoTracking()
            .FirstOrDefaultAsync(rule => rule.OrganizationId == client.OrganizationId, cancellationToken);
        var duplicate = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(other => other.Id != client.Id &&
            other.OrganizationId == client.OrganizationId &&
            (((duplicateRule == null || duplicateRule.MatchTaxId) && normalizedTaxId != null && other.NormalizedTaxId == normalizedTaxId) ||
             ((duplicateRule == null || duplicateRule.MatchName) && other.NormalizedName == normalizedName) ||
             ((duplicateRule == null || duplicateRule.MatchBillingEmail) && normalizedEmail != null && other.NormalizedBillingEmail == normalizedEmail)), cancellationToken);
        if (duplicate is not null && (duplicateRule?.BlockOnMatch ?? true))
            throw new InvalidOperationException($"Posible cliente duplicado: {duplicate.Code} · {duplicate.Name}.");
        if (!await _context.CurrencyConfigs.AsNoTracking().AnyAsync(currency => currency.Code == request.PresentationCurrency && currency.IsActive, cancellationToken))
            throw new InvalidOperationException("La moneda de presentación no está activa.");
        if (request.AccountManagerUserId.HasValue && !await _context.Users.AsNoTracking().AnyAsync(user =>
                user.Id == request.AccountManagerUserId && user.OrganizationId == client.OrganizationId && user.IsActive, cancellationToken))
            throw new InvalidOperationException("El responsable seleccionado no pertenece a la organización.");

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.ClientCategories
                .AsNoTracking()
                .AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }
        }

        client.Name = request.Name;
        client.TradeName = request.TradeName;
        client.TaxId = request.TaxId;
        client.BillingEmail = request.BillingEmail;
        client.BillingAddress = request.BillingAddress;
        client.Phone = request.Phone;
        client.Status = request.Status;
        client.Notes = request.Notes;
        client.CategoryId = request.CategoryId;
        client.PresentationCurrency = request.PresentationCurrency.ToUpperInvariant();
        client.PaymentTermsDays = request.PaymentTermsDays;
        client.CreditLimit = request.CreditLimit;
        client.CommercialTerms = request.CommercialTerms;
        client.AccountManagerUserId = request.AccountManagerUserId;
        client.NormalizedName = normalizedName;
        client.NormalizedTaxId = normalizedTaxId;
        client.NormalizedBillingEmail = normalizedEmail;

        if (request.TagIds is not null)
        {
            var tags = await _context.ClientTags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (tags.Count != request.TagIds.Distinct().Count())
            {
                throw new InvalidOperationException("Una o más etiquetas no existen.");
            }

            client.Tags.Clear();
            foreach (var tag in tags)
            {
                client.Tags.Add(tag);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        if (client.AccountManagerUserId.HasValue)
            client.AccountManagerUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == client.AccountManagerUserId, cancellationToken);

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
