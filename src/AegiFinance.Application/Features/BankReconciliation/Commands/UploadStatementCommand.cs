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

        var attempt = new BankImportAttempt
        {
            Id = Guid.NewGuid(),
            BankAccountId = request.BankAccountId,
            FileName = Path.GetFileName(request.FileName),
            Status = "Processing",
            AttemptedAt = DateTime.UtcNow
        };
        _context.BankImportAttempts.Add(attempt);
        await _context.SaveChangesAsync(cancellationToken);

        List<BankStatementRecord> records;

        try
        {
            using var reader = new StreamReader(request.FileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null
            });
            records = csv.GetRecords<BankStatementRecord>().ToList();
            if (records.Count == 0) throw new InvalidOperationException("No records found in the CSV.");
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            attempt.Status = "Failed";
            attempt.Error = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
            attempt.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("The bank statement could not be imported. Review the file format and try again.", exception);
        }

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

        attempt.Status = "Succeeded";
        attempt.RecordsImported = records.Count;
        attempt.CompletedAt = DateTime.UtcNow;

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
