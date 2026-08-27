using System.Text.Json;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankImports;

public sealed record GetBankImportQuery(Guid Id) : IRequest<BankImportBatchDto>;

public sealed class GetBankImportsQuery : IRequest<IReadOnlyList<BankImportBatchDto>>
{
    public Guid? BankAccountId { get; set; }
    public int Limit { get; set; } = 50;
}

public sealed class GetBankImportsQueryHandler : IRequestHandler<GetBankImportsQuery, IReadOnlyList<BankImportBatchDto>>
{
    private readonly IApplicationDbContext _context;
    public GetBankImportsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<BankImportBatchDto>> Handle(GetBankImportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.BankImportAttempts.AsNoTracking().Include(item => item.BankAccount).AsQueryable();
        if (request.BankAccountId.HasValue) query = query.Where(item => item.BankAccountId == request.BankAccountId);
        var attempts = await query.OrderByDescending(item => item.AttemptedAt).Take(Math.Clamp(request.Limit, 1, 100)).ToListAsync(cancellationToken);
        return attempts.Select(item => BankImportQueries.MapSummary(item, item.BankAccount.Name)).ToList();
    }
}

public sealed class GetBankImportQueryHandler : IRequestHandler<GetBankImportQuery, BankImportBatchDto>
{
    private readonly IApplicationDbContext _context;
    public GetBankImportQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<BankImportBatchDto> Handle(GetBankImportQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.BankImportAttempts.AsNoTracking().Include(item => item.BankAccount).Include(item => item.Rows)
            .SingleOrDefaultAsync(item => item.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("El lote de importación no existe.");
        return BankImportQueries.Map(attempt, attempt.BankAccount.Name);
    }
}

public sealed class GetBankImportProfilesQuery : IRequest<IReadOnlyList<BankImportProfileDto>>;

public sealed class GetBankImportProfilesQueryHandler : IRequestHandler<GetBankImportProfilesQuery, IReadOnlyList<BankImportProfileDto>>
{
    private readonly IApplicationDbContext _context;
    public GetBankImportProfilesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<IReadOnlyList<BankImportProfileDto>> Handle(GetBankImportProfilesQuery request, CancellationToken cancellationToken) =>
        await _context.BankImportProfiles.AsNoTracking().Where(item => item.IsActive).OrderBy(item => item.Name)
            .Select(item => new BankImportProfileDto
            {
                Id = item.Id, Name = item.Name, AdapterCode = item.AdapterCode, DateColumn = item.DateColumn,
                DescriptionColumn = item.DescriptionColumn, ReferenceColumn = item.ReferenceColumn, AmountColumn = item.AmountColumn,
                DebitColumn = item.DebitColumn, CreditColumn = item.CreditColumn, CurrencyColumn = item.CurrencyColumn,
                BalanceColumn = item.BalanceColumn, DateFormat = item.DateFormat, Delimiter = item.Delimiter, HeaderRow = item.HeaderRow
            }).ToListAsync(cancellationToken);
}

internal static class BankImportQueries
{
    public static BankImportBatchDto MapSummary(BankImportAttempt attempt, string accountName) => new()
    {
        Id = attempt.Id, BankAccountId = attempt.BankAccountId, BankAccountName = accountName, FileName = attempt.FileName,
        FileHash = attempt.FileHash, AdapterCode = attempt.AdapterCode, Status = attempt.Status, TotalRecords = attempt.TotalRecords,
        ValidRecords = attempt.ValidRecords, DuplicateRecords = attempt.DuplicateRecords, IncompleteRecords = attempt.IncompleteRecords,
        RejectedRecords = attempt.RejectedRecords, RecordsImported = attempt.RecordsImported, BankStatementId = attempt.BankStatementId,
        AttemptedAt = attempt.AttemptedAt, CompletedAt = attempt.CompletedAt, Rows = []
    };

    public static BankImportBatchDto Map(BankImportAttempt attempt, string accountName) => new()
    {
        Id = attempt.Id,
        BankAccountId = attempt.BankAccountId,
        BankAccountName = accountName,
        FileName = attempt.FileName,
        FileHash = attempt.FileHash,
        AdapterCode = attempt.AdapterCode,
        Status = attempt.Status,
        TotalRecords = attempt.TotalRecords,
        ValidRecords = attempt.ValidRecords,
        DuplicateRecords = attempt.DuplicateRecords,
        IncompleteRecords = attempt.IncompleteRecords,
        RejectedRecords = attempt.RejectedRecords,
        RecordsImported = attempt.RecordsImported,
        BankStatementId = attempt.BankStatementId,
        AttemptedAt = attempt.AttemptedAt,
        CompletedAt = attempt.CompletedAt,
        Rows = attempt.Rows.OrderBy(row => row.RowNumber).Select(row => new BankImportRowDto
        {
            Id = row.Id, RowNumber = row.RowNumber, TransactionDate = row.TransactionDate, Description = row.Description,
            Reference = row.Reference, Amount = row.Amount, Currency = row.Currency, Balance = row.Balance, Status = row.Status,
            Issues = DeserializeIssues(row.IssuesJson)
        }).ToList()
    };

    private static IReadOnlyList<BankImportIssueDto> DeserializeIssues(string json)
    {
        try { return JsonSerializer.Deserialize<List<BankImportIssueDto>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? []; }
        catch (JsonException) { return []; }
    }
}
