using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Domain.Accounting;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.BankImports;

public sealed class PreviewBankImportCommand : IRequest<BankImportBatchDto>
{
    public Guid BankAccountId { get; set; }
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
    public string AdapterCode { get; set; } = "generic";
    public Guid? ProfileId { get; set; }
    public BankImportColumnMap? Columns { get; set; }
}

public sealed class PreviewBankImportCommandHandler : IRequestHandler<PreviewBankImportCommand, BankImportBatchDto>
{
    private readonly IApplicationDbContext _context;

    public PreviewBankImportCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<BankImportBatchDto> Handle(PreviewBankImportCommand request, CancellationToken cancellationToken)
    {
        if (request.Content.Length == 0) throw new InvalidOperationException("El archivo está vacío.");
        if (request.Content.Length > 10 * 1024 * 1024) throw new InvalidOperationException("El archivo excede el límite de 10 MB.");

        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (extension is not ".csv" and not ".xlsx") throw new InvalidOperationException("Sólo se admiten archivos CSV o XLSX.");
        if (!BankImportMapping.Adapters.Any(item => item.Code == request.AdapterCode)) throw new InvalidOperationException("El adaptador bancario no es válido.");

        var account = await _context.BankAccounts.AsNoTracking().SingleOrDefaultAsync(item => item.Id == request.BankAccountId, cancellationToken)
            ?? throw new KeyNotFoundException("La cuenta bancaria no existe o no pertenece a la organización actual.");
        if (!account.IsActive) throw new InvalidOperationException("La cuenta bancaria está inactiva.");

        BankImportProfile? profile = null;
        if (request.ProfileId.HasValue)
            profile = await _context.BankImportProfiles.AsNoTracking().SingleOrDefaultAsync(item => item.Id == request.ProfileId, cancellationToken)
                ?? throw new KeyNotFoundException("El perfil de importación no existe.");

        var attempt = new BankImportAttempt
        {
            Id = Guid.NewGuid(),
            BankAccountId = account.Id,
            FileName = Path.GetFileName(request.FileName),
            FileHash = Convert.ToHexString(SHA256.HashData(request.Content)).ToLowerInvariant(),
            FileSize = request.Content.LongLength,
            FileType = extension[1..],
            AdapterCode = request.AdapterCode,
            ProfileId = profile?.Id,
            Status = BankImportStatus.Preview,
            AttemptedAt = DateTime.UtcNow
        };
        _context.BankImportAttempts.Add(attempt);

        try
        {
            var sourceRows = extension == ".csv"
                ? ReadCsv(request.Content, profile?.Delimiter)
                : ReadWorkbook(request.Content, profile?.HeaderRow ?? 1);
            if (sourceRows.Headers.Count == 0) throw new InvalidOperationException("No se encontró una fila de encabezados.");
            if (sourceRows.Rows.Count == 0) throw new InvalidOperationException("El archivo no contiene movimientos.");
            if (sourceRows.Rows.Count > 10_000) throw new InvalidOperationException("El archivo excede el límite de 10,000 movimientos por lote.");

            var columns = BankImportMapping.Resolve(sourceRows.Headers, request.Columns, profile, profile?.AdapterCode ?? request.AdapterCode);
            if (columns.Date is null || columns.Description is null || (columns.Amount is null && columns.Debit is null && columns.Credit is null))
                throw new InvalidOperationException("No se identificaron las columnas obligatorias: fecha, descripción y monto, o cargo/abono. Seleccioná un perfil o asigná las columnas.");

            var existingHashes = await _context.BankStatementLines.AsNoTracking()
                .Where(item => item.DeduplicationHash != null)
                .Select(item => item.DeduplicationHash!)
                .ToHashSetAsync(cancellationToken);
            var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
            var seenHashes = new HashSet<string>(StringComparer.Ordinal);

            foreach (var source in sourceRows.Rows)
            {
                var issues = new List<BankImportIssueDto>();
                var dateText = BankImportMapping.Value(source.Values, columns.Date);
                var description = BankImportMapping.Value(source.Values, columns.Description);
                var reference = BankImportMapping.Value(source.Values, columns.Reference);
                var currency = (BankImportMapping.Value(source.Values, columns.Currency) ?? account.Currency).Trim().ToUpperInvariant();
                var balanceText = BankImportMapping.Value(source.Values, columns.Balance);
                DateTime? date = null;
                decimal? amount = null;
                decimal? balance = null;

                if (string.IsNullOrWhiteSpace(dateText)) issues.Add(Issue("date", "La fecha está vacía.", "Capturá una fecha válida en esta fila."));
                else if (BankImportMapping.TryDate(dateText, profile?.DateFormat, out var parsedDate)) date = parsedDate.Date;
                else issues.Add(Issue("date", $"No se reconoce la fecha '{dateText}'.", "Usá una fecha como 2026-08-26 o definí el formato en el perfil."));

                if (string.IsNullOrWhiteSpace(description)) issues.Add(Issue("description", "La descripción está vacía.", "Agregá el concepto del movimiento."));
                else if (description.Length > 1000) issues.Add(Issue("description", "La descripción excede 1000 caracteres.", "Reducí la descripción a 1000 caracteres o menos."));

                var amountText = BankImportMapping.Value(source.Values, columns.Amount);
                if (!string.IsNullOrWhiteSpace(amountText))
                {
                    if (BankImportMapping.TryAmount(amountText, out var parsedAmount)) amount = parsedAmount;
                    else issues.Add(Issue("amount", $"No se reconoce el importe '{amountText}'.", "Usá un número con hasta dos decimales."));
                }
                else
                {
                    var debitText = BankImportMapping.Value(source.Values, columns.Debit);
                    var creditText = BankImportMapping.Value(source.Values, columns.Credit);
                    var hasDebit = BankImportMapping.TryAmount(debitText, out var debit);
                    var hasCredit = BankImportMapping.TryAmount(creditText, out var credit);
                    if (hasDebit && hasCredit && debit != 0 && credit != 0) issues.Add(Issue("amount", "La fila contiene cargo y abono simultáneamente.", "Dejá sólo uno de los dos importes."));
                    else if (hasCredit) amount = Math.Abs(credit);
                    else if (hasDebit) amount = -Math.Abs(debit);
                    else issues.Add(Issue("amount", "El importe está vacío.", "Capturá monto o una columna de cargo/abono."));
                }
                if (amount == 0) issues.Add(Issue("amount", "El importe no puede ser cero.", "Indicá el valor real del movimiento."));

                if (currency.Length != 3 || !currency.All(char.IsLetter)) issues.Add(Issue("currency", $"La moneda '{currency}' no es válida.", "Usá un código ISO de tres letras, por ejemplo MXN."));
                else if (!string.Equals(currency, account.Currency, StringComparison.OrdinalIgnoreCase)) issues.Add(Issue("currency", $"La moneda {currency} no coincide con la cuenta {account.Currency}.", "Importá el movimiento en una cuenta de la misma moneda."));

                if (!string.IsNullOrWhiteSpace(balanceText))
                {
                    if (BankImportMapping.TryAmount(balanceText, out var parsedBalance)) balance = parsedBalance;
                    else issues.Add(Issue("balance", $"No se reconoce el saldo '{balanceText}'.", "Usá un número con hasta dos decimales."));
                }

                var status = issues.Count == 0 ? BankImportRowStatus.Valid :
                    issues.Any(item => item.Message.Contains("vacía", StringComparison.OrdinalIgnoreCase)) ? BankImportRowStatus.Incomplete : BankImportRowStatus.Rejected;
                string? deduplicationHash = null;
                if (date.HasValue && amount.HasValue && !string.IsNullOrWhiteSpace(description) && currency.Length == 3)
                {
                    var canonical = BankImportMapping.Hash(account.Id.ToString(), date.Value.ToString("O"), description, reference, amount.Value.ToString("0.00", CultureInfo.InvariantCulture), currency);
                    occurrences.TryGetValue(canonical, out var occurrence);
                    occurrence++;
                    occurrences[canonical] = occurrence;
                    deduplicationHash = BankImportRules.DeduplicationHash(account.Id, date.Value, description, reference, amount.Value, currency, occurrence);
                    if (existingHashes.Contains(deduplicationHash) || !seenHashes.Add(deduplicationHash)) status = BankImportRowStatus.Duplicate;
                }

                var row = new BankImportRow
                {
                    Id = Guid.NewGuid(),
                    ImportAttemptId = attempt.Id,
                    RowNumber = source.RowNumber,
                    RawDataJson = BankImportMapping.Json(source.Values),
                    TransactionDate = date,
                    Description = description,
                    Reference = string.IsNullOrWhiteSpace(reference) ? null : reference.Length > 200 ? reference[..200] : reference,
                    Amount = amount,
                    Currency = currency,
                    Balance = balance,
                    DeduplicationHash = deduplicationHash,
                    Status = status,
                    IssuesJson = BankImportMapping.Json(issues)
                };
                attempt.Rows.Add(row);
            }

            RefreshCounts(attempt);
            await _context.SaveChangesAsync(cancellationToken);
            return BankImportQueries.Map(attempt, account.Name);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            attempt.Status = BankImportStatus.Failed;
            attempt.Error = exception.Message.Length > 2000 ? exception.Message[..2000] : exception.Message;
            attempt.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException(exception.Message, exception);
        }
    }

    private static BankImportIssueDto Issue(string field, string message, string correction) => new() { Field = field, Message = message, Correction = correction };

    private static void RefreshCounts(BankImportAttempt attempt)
    {
        attempt.TotalRecords = attempt.Rows.Count;
        attempt.ValidRecords = attempt.Rows.Count(item => item.Status == BankImportRowStatus.Valid);
        attempt.DuplicateRecords = attempt.Rows.Count(item => item.Status == BankImportRowStatus.Duplicate);
        attempt.IncompleteRecords = attempt.Rows.Count(item => item.Status == BankImportRowStatus.Incomplete);
        attempt.RejectedRecords = attempt.Rows.Count(item => item.Status == BankImportRowStatus.Rejected);
    }

    private static SourceData ReadCsv(byte[] content, string? configuredDelimiter)
    {
        using var stream = new MemoryStream(content);
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), true);
        var sample = reader.ReadLine() ?? string.Empty;
        stream.Position = 0;
        reader.DiscardBufferedData();
        var delimiter = !string.IsNullOrEmpty(configuredDelimiter) ? configuredDelimiter : new[] { ",", ";", "\t" }.OrderByDescending(item => sample.Count(character => character == item[0])).First();
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = TrimOptions.Trim
        });
        if (!csv.Read() || !csv.ReadHeader()) return new SourceData([], []);
        var headers = UniqueHeaders(csv.HeaderRecord?.Select(Header) ?? []);
        var rows = new List<SourceRow>();
        while (csv.Read())
        {
            var values = headers.Select((header, index) => new { header, value = csv.GetField(index) ?? string.Empty })
                .ToDictionary(item => item.header, item => item.value, StringComparer.OrdinalIgnoreCase);
            if (values.Values.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(new SourceRow(csv.Parser.Row, values));
        }
        return new SourceData(headers, rows);
    }

    private static SourceData ReadWorkbook(byte[] content, int headerRow)
    {
        using var stream = new MemoryStream(content);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault() ?? throw new InvalidOperationException("El libro no contiene hojas.");
        var used = worksheet.RangeUsed() ?? throw new InvalidOperationException("La primera hoja está vacía.");
        if (headerRow < used.RangeAddress.FirstAddress.RowNumber || headerRow > used.RangeAddress.LastAddress.RowNumber) throw new InvalidOperationException("La fila de encabezados configurada está fuera del rango usado.");
        var firstColumn = used.RangeAddress.FirstAddress.ColumnNumber;
        var lastColumn = used.RangeAddress.LastAddress.ColumnNumber;
        var headers = UniqueHeaders(Enumerable.Range(firstColumn, lastColumn - firstColumn + 1).Select(column => Header(worksheet.Cell(headerRow, column).GetFormattedString())));
        var rows = new List<SourceRow>();
        for (var rowNumber = headerRow + 1; rowNumber <= used.RangeAddress.LastAddress.RowNumber; rowNumber++)
        {
            var values = headers.Select((header, index) => new { header, value = worksheet.Cell(rowNumber, firstColumn + index).GetFormattedString() })
                .ToDictionary(item => item.header, item => item.value, StringComparer.OrdinalIgnoreCase);
            if (values.Values.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(new SourceRow(rowNumber, values));
        }
        return new SourceData(headers, rows);
    }

    private static string Header(string? value)
    {
        var header = value?.Trim() ?? string.Empty;
        return string.IsNullOrWhiteSpace(header) ? $"Column_{Guid.NewGuid():N}" : header;
    }

    private static IReadOnlyList<string> UniqueHeaders(IEnumerable<string> source)
    {
        var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var result = new List<string>();
        foreach (var header in source)
        {
            counts.TryGetValue(header, out var count);
            count++;
            counts[header] = count;
            result.Add(count == 1 ? header : $"{header}_{count}");
        }
        return result;
    }

    private sealed record SourceData(IReadOnlyList<string> Headers, IReadOnlyList<SourceRow> Rows);
    private sealed record SourceRow(int RowNumber, IReadOnlyDictionary<string, string> Values);
}
