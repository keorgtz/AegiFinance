using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.CreateService;

public class CreateServiceCommand : IRequest<ServiceDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public BillingType BillingType { get; set; } = BillingType.Monthly;
    public decimal DefaultPrice { get; set; }
    public string Currency { get; set; } = "MXN";
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; }
}
