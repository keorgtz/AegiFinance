using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;
using MediatR;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientFinancialSummary;

public sealed class GetClientFinancialSummaryQueryHandler : IRequestHandler<GetClientFinancialSummaryQuery, FinancialSummaryDto>
{
    private readonly IMediator _mediator;
    public GetClientFinancialSummaryQueryHandler(IMediator mediator) => _mediator = mediator;
    public async Task<FinancialSummaryDto> Handle(GetClientFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        var statement = await _mediator.Send(new GetClientStatementQuery
        {
            ClientId = request.ClientId, From = request.From, To = request.To,
            Currency = request.Currency, SubscriptionId = request.SubscriptionId
        }, cancellationToken);
        return new FinancialSummaryDto
        {
            ClientId = statement.ClientId, ClientName = statement.ClientName, DisplayCurrency = statement.DisplayCurrency,
            TotalCharges = statement.TotalCharges, TotalPayments = statement.TotalPayments,
            TotalAdjustments = statement.TotalAdjustments, CurrentBalance = statement.FinalBalance
        };
    }
}
