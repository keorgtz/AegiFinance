using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;
using MediatR;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientMovements;

public class GetClientMovementsQueryHandler : IRequestHandler<GetClientMovementsQuery, List<AccountStatementItemDto>>
{
    private readonly IMediator _mediator;

    public GetClientMovementsQueryHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<List<AccountStatementItemDto>> Handle(GetClientMovementsQuery request, CancellationToken cancellationToken)
    {
        var statement = await _mediator.Send(new GetClientStatementQuery
        {
            ClientId = request.ClientId,
            From = request.From,
            To = request.To,
            Currency = request.Currency
        }, cancellationToken);

        return statement.Items;
    }
}
