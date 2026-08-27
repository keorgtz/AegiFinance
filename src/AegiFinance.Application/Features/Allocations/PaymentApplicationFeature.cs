using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Allocations;

public sealed class GetPaymentApplicationsQuery : IRequest<IReadOnlyList<PaymentApplicationDto>>
{
    public Guid? ClientId { get; set; }
    public PaymentApplicationStatus? Status { get; set; }
    public int Limit { get; set; } = 100;
}
public sealed record GetPaymentApplicationQuery(Guid Id) : IRequest<PaymentApplicationDto>;
public sealed class ApplyPaymentsAutomaticallyCommand : IRequest<AllocationResult>
{
    public List<Guid> LedgerEntryIds { get; set; } = [];
    public PaymentApplicationPriority? Priority { get; set; }
    public Guid? PreferredServiceId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
public sealed class ApplyPaymentsManuallyCommand : IRequest<AllocationResult>
{
    public List<PaymentAllocationLineRequest> Allocations { get; set; } = [];
    public string IdempotencyKey { get; set; } = string.Empty;
}
public sealed record ReversePaymentApplicationCommand(Guid Id, string Reason) : IRequest;
public sealed class ReapplyPaymentApplicationCommand : IRequest<AllocationResult>
{
    public Guid Id { get; set; }
    public PaymentApplicationPriority? Priority { get; set; }
    public Guid? PreferredServiceId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
}
public sealed record GetPaymentApplicationSettingsQuery : IRequest<PaymentApplicationSettingsDto>;
public sealed class UpdatePaymentApplicationSettingsCommand : IRequest<PaymentApplicationSettingsDto>
{
    public PaymentApplicationPriority DefaultPriority { get; set; }
}

public sealed class GetPaymentApplicationsQueryHandler : IRequestHandler<GetPaymentApplicationsQuery, IReadOnlyList<PaymentApplicationDto>>
{
    private readonly IApplicationDbContext _context;
    public GetPaymentApplicationsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<IReadOnlyList<PaymentApplicationDto>> Handle(GetPaymentApplicationsQuery request, CancellationToken cancellationToken)
    {
        var query = PaymentApplicationFeature.Query(_context);
        if (request.ClientId.HasValue) query = query.Where(item => item.ClientId == request.ClientId);
        if (request.Status.HasValue) query = query.Where(item => item.Status == request.Status);
        var items = await query.OrderByDescending(item => item.AppliedAt).Take(Math.Clamp(request.Limit, 1, 500)).ToListAsync(cancellationToken);
        return items.Select(PaymentApplicationFeature.Map).ToList();
    }
}

public sealed class GetPaymentApplicationQueryHandler : IRequestHandler<GetPaymentApplicationQuery, PaymentApplicationDto>
{
    private readonly IApplicationDbContext _context;
    public GetPaymentApplicationQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PaymentApplicationDto> Handle(GetPaymentApplicationQuery request, CancellationToken cancellationToken)
    {
        var item = await PaymentApplicationFeature.Query(_context).SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("La aplicación de pago no existe.");
        return PaymentApplicationFeature.Map(item);
    }
}

public sealed class ApplyPaymentsAutomaticallyCommandHandler : IRequestHandler<ApplyPaymentsAutomaticallyCommand, AllocationResult>
{
    private readonly IAllocationService _service;
    public ApplyPaymentsAutomaticallyCommandHandler(IAllocationService service) => _service = service;
    public Task<AllocationResult> Handle(ApplyPaymentsAutomaticallyCommand request, CancellationToken cancellationToken) =>
        _service.ApplyAutomaticallyAsync(new AutoPaymentApplicationRequest(request.LedgerEntryIds, request.Priority, request.PreferredServiceId, request.IdempotencyKey), cancellationToken);
}

public sealed class ApplyPaymentsManuallyCommandHandler : IRequestHandler<ApplyPaymentsManuallyCommand, AllocationResult>
{
    private readonly IAllocationService _service;
    public ApplyPaymentsManuallyCommandHandler(IAllocationService service) => _service = service;
    public Task<AllocationResult> Handle(ApplyPaymentsManuallyCommand request, CancellationToken cancellationToken) =>
        _service.ApplyManuallyAsync(new ManualPaymentApplicationRequest(request.Allocations, request.IdempotencyKey), cancellationToken);
}

public sealed class ReversePaymentApplicationCommandHandler : IRequestHandler<ReversePaymentApplicationCommand>
{
    private readonly IAllocationService _service;
    public ReversePaymentApplicationCommandHandler(IAllocationService service) => _service = service;
    public Task Handle(ReversePaymentApplicationCommand request, CancellationToken cancellationToken) => _service.ReverseApplicationAsync(request.Id, request.Reason, cancellationToken);
}

public sealed class ReapplyPaymentApplicationCommandHandler : IRequestHandler<ReapplyPaymentApplicationCommand, AllocationResult>
{
    private readonly IAllocationService _service;
    public ReapplyPaymentApplicationCommandHandler(IAllocationService service) => _service = service;
    public Task<AllocationResult> Handle(ReapplyPaymentApplicationCommand request, CancellationToken cancellationToken) =>
        _service.ReapplyAsync(request.Id, request.Priority, request.PreferredServiceId, request.IdempotencyKey, cancellationToken);
}

public sealed class GetPaymentApplicationSettingsQueryHandler : IRequestHandler<GetPaymentApplicationSettingsQuery, PaymentApplicationSettingsDto>
{
    private readonly IApplicationDbContext _context;
    public GetPaymentApplicationSettingsQueryHandler(IApplicationDbContext context) => _context = context;
    public async Task<PaymentApplicationSettingsDto> Handle(GetPaymentApplicationSettingsQuery request, CancellationToken cancellationToken)
    {
        var item = await _context.PaymentApplicationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        return new PaymentApplicationSettingsDto { DefaultPriority = item?.DefaultPriority ?? PaymentApplicationPriority.DueDate };
    }
}

public sealed class UpdatePaymentApplicationSettingsCommandHandler : IRequestHandler<UpdatePaymentApplicationSettingsCommand, PaymentApplicationSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public UpdatePaymentApplicationSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<PaymentApplicationSettingsDto> Handle(UpdatePaymentApplicationSettingsCommand request, CancellationToken cancellationToken)
    {
        if (request.DefaultPriority == PaymentApplicationPriority.Selection) throw new InvalidOperationException("Selección es una regla manual y no puede ser la prioridad automática predeterminada.");
        var item = await _context.PaymentApplicationSettings.SingleOrDefaultAsync(cancellationToken);
        if (item is null)
        {
            item = new PaymentApplicationSettings { Id = Guid.NewGuid(), OrganizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.") };
            _context.PaymentApplicationSettings.Add(item);
        }
        item.DefaultPriority = request.DefaultPriority;
        await _context.SaveChangesAsync(cancellationToken);
        return new PaymentApplicationSettingsDto { DefaultPriority = item.DefaultPriority };
    }
}

internal static class PaymentApplicationFeature
{
    public static IQueryable<PaymentApplication> Query(IApplicationDbContext context) => context.PaymentApplications.AsNoTracking()
        .Include(item => item.Client)
        .Include(item => item.Payments).ThenInclude(item => item.LedgerEntry)
        .Include(item => item.Payments).ThenInclude(item => item.JournalEntry)
        .Include(item => item.Allocations).ThenInclude(item => item.BillingItem);

    public static PaymentApplicationDto Map(PaymentApplication item) => new()
    {
        Id = item.Id, ClientId = item.ClientId, ClientName = item.Client.Name, ReceiptNumber = item.ReceiptNumber,
        Priority = item.Priority, Origin = item.Origin, Status = item.Status, TotalPaymentAmount = item.TotalPaymentAmount,
        AppliedAmount = item.AppliedAmount, UnappliedAmount = item.UnappliedAmount, Currency = item.Currency,
        AppliedAt = item.AppliedAt, AppliedBy = item.AppliedBy, ReversedAt = item.ReversedAt, ReversalReason = item.ReversalReason,
        ReappliesPaymentApplicationId = item.ReappliesPaymentApplicationId,
        Payments = item.Payments.Select(payment => new PaymentApplicationPaymentDto
        {
            LedgerEntryId = payment.LedgerEntryId, Description = payment.LedgerEntry.Description, Reference = payment.LedgerEntry.Reference,
            Date = payment.LedgerEntry.Date, JournalEntryId = payment.JournalEntryId, JournalEntryNumber = payment.JournalEntry.EntryNumber,
            AvailableBefore = payment.AvailableBefore, AppliedAmount = payment.AppliedAmount, UnappliedAfter = payment.UnappliedAfter
        }).ToList(),
        Allocations = item.Allocations.Select(allocation => new PaymentApplicationLineDto
        {
            Id = allocation.Id, LedgerEntryId = allocation.LedgerEntryId, BillingItemId = allocation.BillingItemId,
            BillingItemDescription = allocation.BillingItem.Description, Amount = allocation.Amount, IsReversed = allocation.IsReversed
        }).ToList()
    };
}
