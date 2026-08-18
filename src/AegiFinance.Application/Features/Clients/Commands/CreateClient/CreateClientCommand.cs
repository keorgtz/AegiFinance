using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommand : IRequest<ClientDto>
{
    public string Name { get; set; } = string.Empty;
    public string? TradeName { get; set; }
    public string? TaxId { get; set; }
    public string? BillingEmail { get; set; }
    public string? BillingAddress { get; set; }
    public string? Phone { get; set; }
    public ClientStatus Status { get; set; } = ClientStatus.Active;
    public string? Notes { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? TagIds { get; set; }
    public string PresentationCurrency { get; set; } = "MXN";
    public int PaymentTermsDays { get; set; }
    public decimal CreditLimit { get; set; }
    public string? CommercialTerms { get; set; }
    public Guid? AccountManagerUserId { get; set; }
}
