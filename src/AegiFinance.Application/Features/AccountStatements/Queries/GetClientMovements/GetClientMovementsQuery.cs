using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientMovements;

public class GetClientMovementsQuery : IRequest<List<AccountStatementItemDto>>
{
    public Guid ClientId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Currency { get; set; } = "MXN";
    public Guid? SubscriptionId { get; set; }
}
