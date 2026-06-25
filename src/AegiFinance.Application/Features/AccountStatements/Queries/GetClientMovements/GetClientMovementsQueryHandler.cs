using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.AccountStatements.Queries.GetClientStatement;
using MediatR;

namespace AegiFinance.Application.Features.AccountStatements.Queries.GetClientMovements;

public class GetClientMovementsQueryHandler : IRequestHandler<GetClientMovementsQuery, List<AccountStatementItemDto>>
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GetClientMovementsQueryHandler(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    public async Task<List<AccountStatementItemDto>> Handle(GetClientMovementsQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para consultar los movimientos del cliente.");
        }

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
