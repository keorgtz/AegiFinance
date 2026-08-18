using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public sealed class ClientGovernanceService : IClientGovernanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public ClientGovernanceService(ApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task<IReadOnlyList<ClientTimelineItemDto>> GetTimelineAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(item => item.Id == clientId, cancellationToken)
            ?? throw new KeyNotFoundException("El cliente no existe.");
        var relatedIds = await _context.ClientContacts.IgnoreQueryFilters().AsNoTracking().Where(item => item.ClientId == client.Id).Select(item => item.Id.ToString()).ToListAsync(cancellationToken);
        relatedIds.AddRange(await _context.ClientNotes.IgnoreQueryFilters().AsNoTracking().Where(item => item.ClientId == client.Id).Select(item => item.Id.ToString()).ToListAsync(cancellationToken));
        relatedIds.AddRange(await _context.ClientDocuments.IgnoreQueryFilters().AsNoTracking().Where(item => item.ClientId == client.Id).Select(item => item.Id.ToString()).ToListAsync(cancellationToken));
        relatedIds.Add(client.Id.ToString());
        var logs = await _context.AuditLogs.AsNoTracking().Where(log => relatedIds.Contains(log.EntityId))
            .OrderByDescending(log => log.Timestamp).Take(100).ToListAsync(cancellationToken);
        return logs.Select(log => new ClientTimelineItemDto(log.Id.ToString(), log.Timestamp, log.EntityType,
            $"{Translate(log.Action)} · {Translate(log.EntityType)}", null, log.UserId)).ToList();
    }

    public async Task<ClientDuplicateRuleDto> GetDuplicateRuleAsync(CancellationToken cancellationToken = default)
    {
        var organizationId = RequireOrganization();
        var rule = await _context.ClientDuplicateRules.AsNoTracking().FirstOrDefaultAsync(item => item.OrganizationId == organizationId, cancellationToken);
        return rule is null ? new(true, true, true, true) : new(rule.MatchTaxId, rule.MatchName, rule.MatchBillingEmail, rule.BlockOnMatch);
    }

    public async Task<ClientDuplicateRuleDto> UpdateDuplicateRuleAsync(ClientDuplicateRuleDto input, CancellationToken cancellationToken = default)
    {
        var organizationId = RequireOrganization();
        var rule = await _context.ClientDuplicateRules.FirstOrDefaultAsync(item => item.OrganizationId == organizationId, cancellationToken);
        if (rule is null) { rule = new ClientDuplicateRule { Id = Guid.NewGuid(), OrganizationId = organizationId }; _context.ClientDuplicateRules.Add(rule); }
        rule.MatchTaxId = input.MatchTaxId; rule.MatchName = input.MatchName; rule.MatchBillingEmail = input.MatchBillingEmail; rule.BlockOnMatch = input.BlockOnMatch;
        await _context.SaveChangesAsync(cancellationToken);
        return input;
    }

    public async Task<IReadOnlyList<ClientDuplicateMatchDto>> FindDuplicatesAsync(string name, string? taxId, string? billingEmail, Guid? excludeClientId, CancellationToken cancellationToken = default)
    {
        var organizationId = RequireOrganization();
        var rule = await GetDuplicateRuleAsync(cancellationToken);
        var normalizedName = ClientIdentityRules.Normalize(name); var normalizedTax = ClientIdentityRules.NormalizeOptional(taxId); var normalizedEmail = ClientIdentityRules.NormalizeOptional(billingEmail);
        var candidates = await _context.Clients.AsNoTracking().Where(item => item.OrganizationId == organizationId && item.Id != excludeClientId).ToListAsync(cancellationToken);
        return candidates.Select(client => new { client, fields = new List<string>() })
            .Select(item => { if (rule.MatchTaxId && normalizedTax != null && item.client.NormalizedTaxId == normalizedTax) item.fields.Add("RFC / ID fiscal"); if (rule.MatchName && item.client.NormalizedName == normalizedName) item.fields.Add("Nombre"); if (rule.MatchBillingEmail && normalizedEmail != null && item.client.NormalizedBillingEmail == normalizedEmail) item.fields.Add("Correo"); return item; })
            .Where(item => item.fields.Count > 0).Select(item => new ClientDuplicateMatchDto(item.client.Id, item.client.Code, item.client.Name, item.fields)).ToList();
    }

    private Guid RequireOrganization() => _currentUser.OrganizationId ?? throw new InvalidOperationException("El usuario no tiene una organización asignada.");
    private static string Translate(string value) => value switch { "Created" => "Creación", "Updated" => "Actualización", "Deleted" => "Eliminación", "Client" => "cliente", "ClientContact" => "contacto", "ClientNote" => "nota", "ClientDocument" => "documento", _ => value };
}
