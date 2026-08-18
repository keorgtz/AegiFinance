using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Dashboard;

public sealed class GetUnreconciledBankLinesQuery : IRequest<List<UnreconciledBankLineDto>>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? BankAccountId { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class GetUnreconciledBankLinesQueryHandler : IRequestHandler<GetUnreconciledBankLinesQuery, List<UnreconciledBankLineDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUnreconciledBankLinesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<UnreconciledBankLineDto>> Handle(GetUnreconciledBankLinesQuery request, CancellationToken cancellationToken)
    {
        var from = (request.From ?? DateTime.UtcNow.Date.AddDays(-29)).Date;
        var to = (request.To ?? DateTime.UtcNow.Date).Date.AddDays(1);
        var query = _context.BankStatementLines.AsNoTracking()
            .Where(item => !item.IsReconciled && item.TransactionDate >= from && item.TransactionDate < to &&
                item.BankStatement.BankAccount.Currency == request.Currency.ToUpperInvariant());
        if (request.BankAccountId.HasValue)
            query = query.Where(item => item.BankStatement.BankAccountId == request.BankAccountId.Value);

        return await query.OrderByDescending(item => item.TransactionDate).Take(100)
            .Select(item => new UnreconciledBankLineDto
            {
                Id = item.Id,
                BankAccountName = item.BankStatement.BankAccount.Name,
                TransactionDate = item.TransactionDate,
                Description = item.Description,
                Amount = item.Amount,
                Currency = item.BankStatement.BankAccount.Currency,
                Reference = item.Reference
            }).ToListAsync(cancellationToken);
    }
}
