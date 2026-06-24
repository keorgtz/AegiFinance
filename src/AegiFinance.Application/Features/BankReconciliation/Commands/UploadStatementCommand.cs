using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using System.Globalization;

namespace AegiFinance.Application.Features.BankReconciliation.Commands;

public class UploadStatementCommand : IRequest<Guid>
{
    public Guid BankAccountId { get; set; }
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
}

public class UploadStatementCommandHandler : IRequestHandler<UploadStatementCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public UploadStatementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UploadStatementCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _context.BankAccounts.FindAsync(new object[] { request.BankAccountId }, cancellationToken);
        if (bankAccount == null)
            throw new Exception("Bank account not found.");

        var records = new List<BankStatementRecord>();

        using (var reader = new StreamReader(request.FileStream))
        using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        }))
        {
            // Esperamos un CSV con Date, Description, Reference, Amount
            records = csv.GetRecords<BankStatementRecord>().ToList();
        }

        if (!records.Any())
            throw new Exception("No records found in the CSV.");

        var statement = new BankStatement
        {
            BankAccountId = request.BankAccountId,
            StatementDate = DateTime.UtcNow,
            StartDate = records.Min(r => r.Date),
            EndDate = records.Max(r => r.Date),
            OpeningBalance = 0, // Se asume que el usuario actualizará esto o se calcula
            ClosingBalance = 0,
            FileUrl = request.FileName
        };

        _context.BankStatements.Add(statement);
        
        foreach (var record in records)
        {
            var line = new BankStatementLine
            {
                BankStatementId = statement.Id,
                TransactionDate = record.Date,
                Description = record.Description ?? "No description",
                Reference = record.Reference,
                Amount = record.Amount,
                IsReconciled = false
            };
            
            _context.BankStatementLines.Add(line);
            
            // Generate draft ledger entry
            var ledgerEntry = new LedgerEntry
            {
                BankAccountId = request.BankAccountId,
                Date = record.Date,
                Description = record.Description ?? "Bank Import",
                Reference = record.Reference,
                Amount = record.Amount,
                Currency = bankAccount.Currency,
                EntryType = record.Amount >= 0 ? LedgerEntryType.Income : LedgerEntryType.Expense,
                IsReconciled = false
            };
            
            _context.LedgerEntries.Add(ledgerEntry);
            
            // Link them
            line.LedgerEntry = ledgerEntry;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return statement.Id;
    }
}

public class BankStatementRecord
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal Amount { get; set; }
}
