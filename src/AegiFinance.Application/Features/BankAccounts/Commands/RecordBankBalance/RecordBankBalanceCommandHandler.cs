using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankAccounts.Commands.RecordBankBalance;

public sealed class RecordBankBalanceCommandHandler : IRequestHandler<RecordBankBalanceCommand, BankAccountBalancesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IAccountBalanceCalculator _calculator;

    public RecordBankBalanceCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IAccountBalanceCalculator calculator)
    {
        _context = context;
        _currentUser = currentUser;
        _calculator = calculator;
    }

    public async Task<BankAccountBalancesDto> Handle(RecordBankBalanceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser())
            throw new UnauthorizedAccessException("No tiene permiso para registrar saldos bancarios.");

        var accountExists = await _context.BankAccounts.AsNoTracking()
            .AnyAsync(account => account.Id == request.BankAccountId, cancellationToken);
        if (!accountExists) throw new InvalidOperationException("La cuenta bancaria no existe.");

        var asOfDate = request.AsOfDate.Date;
        var dayEnd = asOfDate.AddDays(1).AddTicks(-1);
        var statement = await _context.BankStatements.FirstOrDefaultAsync(item =>
            item.BankAccountId == request.BankAccountId && item.IsBalanceVerified && item.FileUrl == null &&
            item.StartDate == asOfDate && item.EndDate == dayEnd, cancellationToken);
        if (statement is null)
        {
            statement = new BankStatement
            {
                Id = Guid.NewGuid(), BankAccountId = request.BankAccountId,
                StartDate = asOfDate, EndDate = dayEnd, IsBalanceVerified = true
            };
            _context.BankStatements.Add(statement);
        }
        statement.StatementDate = DateTime.UtcNow;
        statement.OpeningBalance = request.Balance;
        statement.ClosingBalance = request.Balance;
        await _context.SaveChangesAsync(cancellationToken);
        return await _calculator.CalculateBalancesAsync(request.BankAccountId, asOfDate, cancellationToken);
    }
}
