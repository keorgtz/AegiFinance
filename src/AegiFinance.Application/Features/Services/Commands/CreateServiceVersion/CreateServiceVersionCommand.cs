using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.CreateServiceVersion;

public sealed class CreateServiceVersionCommand : IRequest<ServiceVersionDto>
{
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BillingType BillingType { get; set; } = BillingType.Monthly;
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "MXN";
    public decimal DefaultDiscountPercent { get; set; }
    public decimal DefaultTaxPercent { get; set; }
    public int? CustomIntervalDays { get; set; }
    public ProrationPolicy ProrationPolicy { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public string? Terms { get; set; }
    public bool IsPublished { get; set; } = true;
    public List<CreateServiceVersionConceptRequest> Concepts { get; set; } = new();
}

public sealed class CreateServiceVersionConceptRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public int SortOrder { get; set; }
}
