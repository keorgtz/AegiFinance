using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Accounting;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Reconciliation;

public sealed class GetReconciliationSettingsQuery : IRequest<ReconciliationSettingsDto>;
public sealed class UpdateReconciliationSettingsCommand : IRequest<ReconciliationSettingsDto>
{
    public int DateToleranceDays { get; set; }
    public decimal AmountTolerance { get; set; }
    public decimal SuggestionThreshold { get; set; }
    public decimal AutoConfirmThreshold { get; set; }
    public bool AllowAutoConfirmExact { get; set; }
    public int AmountWeight { get; set; }
    public int DateWeight { get; set; }
    public int ReferenceWeight { get; set; }
    public int ClientWeight { get; set; }
    public int PatternWeight { get; set; }
}

internal static class ReconciliationSettingsFeature
{
    public static ReconciliationSettings New(Guid organizationId) => new() { Id = Guid.NewGuid(), OrganizationId = organizationId };
    public static ReconciliationSettingsDto Map(ReconciliationSettings item) => new()
    {
        DateToleranceDays = item.DateToleranceDays, AmountTolerance = item.AmountTolerance,
        SuggestionThreshold = item.SuggestionThreshold, AutoConfirmThreshold = item.AutoConfirmThreshold,
        AllowAutoConfirmExact = item.AllowAutoConfirmExact, AmountWeight = item.AmountWeight, DateWeight = item.DateWeight,
        ReferenceWeight = item.ReferenceWeight, ClientWeight = item.ClientWeight, PatternWeight = item.PatternWeight
    };
}

public sealed class GetReconciliationSettingsQueryHandler : IRequestHandler<GetReconciliationSettingsQuery, ReconciliationSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public GetReconciliationSettingsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<ReconciliationSettingsDto> Handle(GetReconciliationSettingsQuery request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        var item = await _context.ReconciliationSettings.AsNoTracking().SingleOrDefaultAsync(cancellationToken) ?? ReconciliationSettingsFeature.New(organizationId);
        return ReconciliationSettingsFeature.Map(item);
    }
}

public sealed class UpdateReconciliationSettingsCommandHandler : IRequestHandler<UpdateReconciliationSettingsCommand, ReconciliationSettingsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public UpdateReconciliationSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }
    public async Task<ReconciliationSettingsDto> Handle(UpdateReconciliationSettingsCommand request, CancellationToken cancellationToken)
    {
        var organizationId = _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("No hay una organización activa.");
        var item = await _context.ReconciliationSettings.SingleOrDefaultAsync(cancellationToken);
        var isNew = item is null;
        item ??= ReconciliationSettingsFeature.New(organizationId);
        item.DateToleranceDays = request.DateToleranceDays; item.AmountTolerance = request.AmountTolerance;
        item.SuggestionThreshold = request.SuggestionThreshold; item.AutoConfirmThreshold = request.AutoConfirmThreshold;
        item.AllowAutoConfirmExact = request.AllowAutoConfirmExact; item.AmountWeight = request.AmountWeight;
        item.DateWeight = request.DateWeight; item.ReferenceWeight = request.ReferenceWeight;
        item.ClientWeight = request.ClientWeight; item.PatternWeight = request.PatternWeight;
        ReconciliationRules.ValidateSettings(item);
        if (isNew) _context.ReconciliationSettings.Add(item);
        await _context.SaveChangesAsync(cancellationToken);
        return ReconciliationSettingsFeature.Map(item);
    }
}
