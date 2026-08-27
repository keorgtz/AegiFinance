using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.AccountStatements;

public sealed record GetAccountStatementInquiriesQuery(Guid ClientId) : IRequest<IReadOnlyList<AccountStatementInquiryDto>>;
public sealed class CreateAccountStatementInquiryCommand : IRequest<AccountStatementInquiryDto>
{
    public Guid ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public Guid? JournalEntryId { get; set; }
    public string? StatementVerificationCode { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
}
public sealed record ResolveAccountStatementInquiryCommand(Guid Id, string Resolution) : IRequest<AccountStatementInquiryDto>;

public sealed class GetAccountStatementInquiriesQueryHandler : IRequestHandler<GetAccountStatementInquiriesQuery, IReadOnlyList<AccountStatementInquiryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetAccountStatementInquiriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<IReadOnlyList<AccountStatementInquiryDto>> Handle(GetAccountStatementInquiriesQuery request, CancellationToken cancellationToken)
    {
        EnsureClientScope(_currentUser, request.ClientId);
        var query = _context.AccountStatementInquiries.AsNoTracking().Include(item => item.Subscription).Include(item => item.JournalEntry)
            .Where(item => item.ClientId == request.ClientId);
        if (_currentUser.IsClientUser() && _currentUser.UserId.HasValue)
        {
            var allowed = await RestrictedSubscriptionsAsync(_context, _currentUser.UserId.Value, cancellationToken);
            if (allowed.Count > 0) query = query.Where(item => item.SubscriptionId.HasValue && allowed.Contains(item.SubscriptionId.Value));
        }
        return (await query.OrderByDescending(item => item.RequestedAt).Take(100).ToListAsync(cancellationToken)).Select(Map).ToList();
    }

    internal static AccountStatementInquiryDto Map(AccountStatementInquiry item) => new()
    {
        Id = item.Id, ClientId = item.ClientId, SubscriptionId = item.SubscriptionId, SubscriptionCode = item.Subscription?.Code,
        JournalEntryId = item.JournalEntryId, JournalEntryNumber = item.JournalEntry?.EntryNumber,
        StatementVerificationCode = item.StatementVerificationCode, Subject = item.Subject, Message = item.Message,
        Status = item.Status.ToString(), RequestedAt = item.RequestedAt, ResolvedAt = item.ResolvedAt, Resolution = item.Resolution
    };

    internal static void EnsureClientScope(ICurrentUserService currentUser, Guid clientId)
    {
        if (currentUser.IsClientUser() && currentUser.ClientId != clientId)
            throw new UnauthorizedAccessException("No tiene permiso para consultar aclaraciones de otro cliente.");
    }

    internal static async Task<List<Guid>> RestrictedSubscriptionsAsync(IApplicationDbContext context, Guid userId, CancellationToken cancellationToken) =>
        await context.SubscriptionPermissions.AsNoTracking().Where(item => item.UserId == userId).Select(item => item.SubscriptionId).ToListAsync(cancellationToken);
}

public sealed class CreateAccountStatementInquiryCommandHandler : IRequestHandler<CreateAccountStatementInquiryCommand, AccountStatementInquiryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateAccountStatementInquiryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<AccountStatementInquiryDto> Handle(CreateAccountStatementInquiryCommand request, CancellationToken cancellationToken)
    {
        GetAccountStatementInquiriesQueryHandler.EnsureClientScope(_currentUser, request.ClientId);
        var subject = request.Subject.Trim(); var message = request.Message.Trim(); var key = request.IdempotencyKey.Trim();
        if (subject.Length is < 3 or > 200) throw new InvalidOperationException("El asunto debe contener entre 3 y 200 caracteres.");
        if (message.Length is < 10 or > 4000) throw new InvalidOperationException("La aclaración debe contener entre 10 y 4000 caracteres.");
        if (key.Length is < 8 or > 100) throw new InvalidOperationException("La clave de idempotencia no es válida.");
        if (request.StatementVerificationCode?.Trim().Length > 32) throw new InvalidOperationException("El código de verificación no es válido.");
        var existing = await _context.AccountStatementInquiries.AsNoTracking().Include(item => item.Subscription).Include(item => item.JournalEntry)
            .FirstOrDefaultAsync(item => item.IdempotencyKey == key, cancellationToken);
        if (existing is not null)
        {
            if (existing.ClientId != request.ClientId) throw new InvalidOperationException("La clave de idempotencia ya pertenece a otra aclaración.");
            return GetAccountStatementInquiriesQueryHandler.Map(existing);
        }
        if (!await _context.Clients.AsNoTracking().AnyAsync(item => item.Id == request.ClientId, cancellationToken)) throw new KeyNotFoundException("El cliente no existe.");
        if (request.SubscriptionId.HasValue && !await _context.Subscriptions.AsNoTracking().AnyAsync(item => item.Id == request.SubscriptionId && item.ClientId == request.ClientId, cancellationToken))
            throw new KeyNotFoundException("La suscripción no pertenece al cliente.");
        var journalSubscriptionIds = new List<Guid>();
        if (request.JournalEntryId.HasValue)
        {
            if (!await _context.JournalEntries.AsNoTracking().AnyAsync(item => item.Id == request.JournalEntryId && item.ClientId == request.ClientId, cancellationToken))
                throw new KeyNotFoundException("El movimiento no pertenece al cliente.");
            journalSubscriptionIds = await _context.JournalLines.AsNoTracking()
                .Where(item => item.JournalEntryId == request.JournalEntryId && item.BillingItemId.HasValue)
                .Select(item => item.BillingItem!.SubscriptionId).Distinct().ToListAsync(cancellationToken);
            if (request.SubscriptionId.HasValue && journalSubscriptionIds.Count > 0 && !journalSubscriptionIds.Contains(request.SubscriptionId.Value))
                throw new InvalidOperationException("El movimiento no pertenece a la suscripción indicada.");
        }
        if (_currentUser.IsClientUser() && _currentUser.UserId.HasValue && request.SubscriptionId.HasValue)
        {
            var allowed = await GetAccountStatementInquiriesQueryHandler.RestrictedSubscriptionsAsync(_context, _currentUser.UserId.Value, cancellationToken);
            if (allowed.Count > 0 && !allowed.Contains(request.SubscriptionId.Value)) throw new UnauthorizedAccessException("No tiene permiso para consultar esta suscripción.");
        }
        if (_currentUser.IsClientUser() && _currentUser.UserId.HasValue && request.JournalEntryId.HasValue && !request.SubscriptionId.HasValue)
        {
            var allowed = await GetAccountStatementInquiriesQueryHandler.RestrictedSubscriptionsAsync(_context, _currentUser.UserId.Value, cancellationToken);
            if (allowed.Count > 0 && (journalSubscriptionIds.Count == 0 || !journalSubscriptionIds.All(allowed.Contains)))
                throw new UnauthorizedAccessException("No tiene permiso para consultar este movimiento.");
        }
        var item = new AccountStatementInquiry
        {
            Id = Guid.NewGuid(), OrganizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa."),
            ClientId = request.ClientId, SubscriptionId = request.SubscriptionId, JournalEntryId = request.JournalEntryId,
            StatementVerificationCode = request.StatementVerificationCode?.Trim(), Subject = subject, Message = message,
            IdempotencyKey = key, Status = AccountStatementInquiryStatus.Open, RequestedAt = DateTime.UtcNow, RequestedBy = _currentUser.UserId
        };
        _context.AccountStatementInquiries.Add(item);
        try { await _context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            var duplicate = await _context.AccountStatementInquiries.AsNoTracking().Include(value => value.Subscription).Include(value => value.JournalEntry)
                .FirstOrDefaultAsync(value => value.IdempotencyKey == key, cancellationToken);
            if (duplicate is not null)
            {
                if (duplicate.ClientId != request.ClientId) throw new InvalidOperationException("La clave de idempotencia ya pertenece a otra aclaración.");
                return GetAccountStatementInquiriesQueryHandler.Map(duplicate);
            }
            throw;
        }
        return GetAccountStatementInquiriesQueryHandler.Map(item);
    }
}

public sealed class ResolveAccountStatementInquiryCommandHandler : IRequestHandler<ResolveAccountStatementInquiryCommand, AccountStatementInquiryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public ResolveAccountStatementInquiryCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<AccountStatementInquiryDto> Handle(ResolveAccountStatementInquiryCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("Los clientes no pueden resolver sus propias aclaraciones.");
        var resolution = request.Resolution.Trim();
        if (resolution.Length is < 3 or > 4000) throw new InvalidOperationException("La respuesta debe contener entre 3 y 4000 caracteres.");
        var item = await _context.AccountStatementInquiries.Include(value => value.Subscription).Include(value => value.JournalEntry)
            .SingleOrDefaultAsync(value => value.Id == request.Id, cancellationToken) ?? throw new KeyNotFoundException("La aclaración no existe.");
        if (item.Status is AccountStatementInquiryStatus.Resolved or AccountStatementInquiryStatus.Closed)
            throw new InvalidOperationException("La aclaración ya fue resuelta.");
        item.Status = AccountStatementInquiryStatus.Resolved; item.Resolution = resolution;
        item.ResolvedAt = DateTime.UtcNow; item.ResolvedBy = _currentUser.UserId;
        await _context.SaveChangesAsync(cancellationToken);
        return GetAccountStatementInquiriesQueryHandler.Map(item);
    }
}
