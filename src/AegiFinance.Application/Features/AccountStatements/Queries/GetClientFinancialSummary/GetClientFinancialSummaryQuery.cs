using AegiFinance.Application.Dtos;
using MediatR;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientFinancialSummary;

public class GetClientFinancialSummaryQuery : IRequest<FinancialSummaryDto>
{
    public Guid ClientId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string Currency { get; set; } = "MXN";
}
