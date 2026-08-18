namespace AegiFinance.Application.Dtos;

public sealed record ServiceVersionConceptDto(Guid Id, string Code, string Name, string? Description, decimal Quantity, decimal UnitPrice, decimal TaxPercent, int SortOrder);

public sealed record ServiceVersionDto(
    Guid Id,
    Guid ServiceId,
    int VersionNumber,
    string Name,
    string? Description,
    string BillingType,
    decimal BasePrice,
    string Currency,
    decimal DefaultDiscountPercent,
    decimal DefaultTaxPercent,
    int? CustomIntervalDays,
    string ProrationPolicy,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string? Terms,
    bool IsPublished,
    IReadOnlyList<ServiceVersionConceptDto> Concepts);

public sealed record SubscriptionTermsVersionDto(
    Guid Id,
    int VersionNumber,
    Guid? ServiceVersionId,
    DateTime EffectiveFrom,
    DateTime? EffectiveTo,
    string BillingType,
    decimal BasePrice,
    string Currency,
    decimal DiscountPercent,
    decimal TaxPercent,
    int BillingDay,
    int? CustomIntervalDays,
    string ProrationPolicy,
    string? Terms,
    string? Reason,
    decimal Total);
