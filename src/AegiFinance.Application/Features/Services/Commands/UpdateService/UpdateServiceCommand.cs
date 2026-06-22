using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Services.Commands.UpdateService;

public class UpdateServiceCommand : IRequest<ServiceDto>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public BillingType BillingType { get; set; }
    public decimal DefaultPrice { get; set; }
    public string Currency { get; set; } = "MXN";
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
}
