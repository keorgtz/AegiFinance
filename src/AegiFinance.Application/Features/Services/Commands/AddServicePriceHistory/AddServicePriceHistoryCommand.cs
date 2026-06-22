using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.AddServicePriceHistory;

public class AddServicePriceHistoryCommand : IRequest<ServicePriceHistoryDto>
{
    public Guid ServiceId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "MXN";
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
}
