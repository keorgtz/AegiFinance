using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;
using AegiFinance.Domain.Enums;

namespace AegiFinance.Application.Features.Dashboard;

public sealed class GetBankImportAttemptsQuery : IRequest<List<BankImportAttemptDto>>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? Status { get; set; }
    public string Currency { get; set; } = "MXN";
}

public sealed class GetBankImportAttemptsQueryHandler : IRequestHandler<GetBankImportAttemptsQuery, List<BankImportAttemptDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBankImportAttemptsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BankImportAttemptDto>> Handle(GetBankImportAttemptsQuery request, CancellationToken cancellationToken)
    {
        var from = (request.From ?? DateTime.UtcNow.Date.AddDays(-29)).Date;
        var to = (request.To ?? DateTime.UtcNow.Date).Date.AddDays(1);
        var query = _context.BankImportAttempts.AsNoTracking()
            .Where(item => item.AttemptedAt >= from && item.AttemptedAt < to &&
                item.BankAccount.Currency == request.Currency.ToUpperInvariant());

        if (request.BankAccountId.HasValue)
            query = query.Where(item => item.BankAccountId == request.BankAccountId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<BankImportStatus>(request.Status, true, out var status))
            query = query.Where(item => item.Status == status);

        return await query.OrderByDescending(item => item.AttemptedAt).Take(100)
            .Select(item => new BankImportAttemptDto
            {
                Id = item.Id,
                BankAccountName = item.BankAccount.Name,
                FileName = item.FileName,
                Status = item.Status.ToString(),
                Error = item.Error,
                RecordsImported = item.RecordsImported,
                AttemptedAt = item.AttemptedAt
            }).ToListAsync(cancellationToken);
    }
}
