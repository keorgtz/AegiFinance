using System.Globalization;
using System.Text;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Fonts;

namespace AegiFinance.Infrastructure.Services;

public sealed class AccountStatementExporter : IAccountStatementExporter
{
    private static readonly object FontLock = new();

    public byte[] CreatePdf(AccountStatementDto statement)
    {
        EnsureFontResolver();
        var document = new Document();
        document.Info.Title = $"Estado de cuenta {statement.ClientName}";
        document.Info.Subject = $"Verificación {statement.VerificationCode}";
        document.Info.Author = statement.OrganizationName;
        document.Info.Keywords = "estado de cuenta, cuentas por cobrar, Major Ledger";
        var normal = document.Styles[StyleNames.Normal]!;
        normal.Font.Name = AegiFinanceFontResolver.FamilyName;
        normal.Font.Size = 8.5;
        normal.Font.Color = Colors.Black;

        var section = document.AddSection();
        section.PageSetup.Orientation = Orientation.Landscape;
        section.PageSetup.TopMargin = Unit.FromCentimeter(1.3);
        section.PageSetup.BottomMargin = Unit.FromCentimeter(1.3);
        section.PageSetup.LeftMargin = Unit.FromCentimeter(1.3);
        section.PageSetup.RightMargin = Unit.FromCentimeter(1.3);
        AddHeader(section, statement);
        AddSummary(section, statement);
        AddMovements(section, statement);
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.AddText($"Verificación {statement.VerificationCode} · Página ");
        footer.AddPageField();
        footer.AddText(" de ");
        footer.AddNumPagesField();

        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();
        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    public byte[] CreateCsv(AccountStatementDto statement)
    {
        var output = new StringBuilder();
        output.AppendLine($"Verificación,{Csv(statement.VerificationCode)}");
        output.AppendLine($"Cliente,{Csv(statement.ClientName)}");
        output.AppendLine($"Periodo,{Csv(Period(statement))}");
        output.AppendLine($"Moneda,{statement.DisplayCurrency}");
        output.AppendLine($"Saldo inicial,{Amount(statement.InitialBalance)}");
        output.AppendLine($"Cargos,{Amount(statement.TotalCharges)}");
        output.AppendLine($"Pagos,{Amount(statement.TotalPayments)}");
        output.AppendLine($"Ajustes netos,{Amount(statement.TotalAdjustments)}");
        output.AppendLine($"Saldo final,{Amount(statement.FinalBalance)}");
        output.AppendLine($"Vencido,{Amount(statement.OverdueBalance)}");
        output.AppendLine();
        output.AppendLine("Fecha,Tipo,Asiento,Plan,Descripción,Referencia,Cargo,Abono,Saldo,Moneda");
        foreach (var item in statement.Items)
            output.Append(item.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',').Append(Csv(item.Type)).Append(',')
                .Append(Csv(item.JournalEntryNumber)).Append(',').Append(Csv(item.SubscriptionCode)).Append(',').Append(Csv(item.Description)).Append(',')
                .Append(Csv(item.Reference)).Append(',').Append(Amount(item.Debit)).Append(',').Append(Amount(item.Credit)).Append(',')
                .Append(Amount(item.Balance)).Append(',').Append(item.Currency).AppendLine();
        return new UTF8Encoding(true).GetBytes(output.ToString());
    }

    private static void AddHeader(Section section, AccountStatementDto statement)
    {
        var title = section.AddParagraph("Estado de cuenta");
        title.Format.Font.Size = 18; title.Format.Font.Bold = true; title.Format.Font.Color = Color.FromRgb(91, 67, 166);
        var client = section.AddParagraph();
        client.AddFormattedText(statement.ClientName, TextFormat.Bold);
        client.AddText($" · {statement.DisplayCurrency} · {Period(statement)}");
        if (!string.IsNullOrWhiteSpace(statement.SubscriptionCode)) client.AddText($" · {statement.SubscriptionCode} / {statement.ServiceName}");
        var metadata = section.AddParagraph($"Generado {statement.StatementDate:yyyy-MM-dd HH:mm} UTC · Verificación {statement.VerificationCode}");
        metadata.Format.Font.Size = 7.5; metadata.Format.Font.Color = Colors.Gray;
        section.AddParagraph(statement.ScopeDescription).Format.SpaceAfter = Unit.FromCentimeter(0.35);
    }

    private static void AddSummary(Section section, AccountStatementDto statement)
    {
        var table = section.AddTable(); table.Borders.Width = 0.5; table.Borders.Color = Colors.LightGray;
        foreach (var _ in Enumerable.Range(0, 6)) table.AddColumn(Unit.FromCentimeter(4.3));
        var labels = new[] { "Saldo inicial", "Cargos", "Pagos", "Ajustes netos", "Saldo final", "Vencido" };
        var values = new[] { statement.InitialBalance, statement.TotalCharges, statement.TotalPayments, statement.TotalAdjustments, statement.FinalBalance, statement.OverdueBalance };
        var labelRow = table.AddRow(); var valueRow = table.AddRow();
        for (var index = 0; index < labels.Length; index++)
        {
            labelRow.Cells[index].AddParagraph(labels[index]).Format.Font.Bold = true;
            valueRow.Cells[index].AddParagraph($"{values[index]:N2} {statement.DisplayCurrency}").Format.Font.Size = 10;
            labelRow.Cells[index].Shading.Color = Color.FromRgb(242, 239, 251);
        }
        table.Format.SpaceAfter = Unit.FromCentimeter(0.45);
    }

    private static void AddMovements(Section section, AccountStatementDto statement)
    {
        var heading = section.AddParagraph("Movimientos"); heading.Format.Font.Size = 11; heading.Format.Font.Bold = true;
        heading.Format.SpaceAfter = Unit.FromCentimeter(0.15);
        var table = section.AddTable(); table.Borders.Width = 0.35; table.Borders.Color = Colors.LightGray;
        var widths = new[] { 2.2, 2.2, 3.2, 3.1, 7.7, 2.8, 2.8, 3.0 };
        foreach (var width in widths) table.AddColumn(Unit.FromCentimeter(width));
        var header = table.AddRow(); header.HeadingFormat = true; header.Format.Font.Bold = true; header.Shading.Color = Color.FromRgb(242, 239, 251);
        var labels = new[] { "Fecha", "Tipo", "Asiento", "Plan", "Descripción", "Cargo", "Abono", "Saldo" };
        for (var index = 0; index < labels.Length; index++) header.Cells[index].AddParagraph(labels[index]);
        foreach (var item in statement.Items)
        {
            var row = table.AddRow();
            var values = new[] { item.Date.ToString("yyyy-MM-dd"), item.Type, item.JournalEntryNumber, item.SubscriptionCode ?? "—", item.Description,
                item.Debit == 0 ? "—" : item.Debit.ToString("N2"), item.Credit == 0 ? "—" : item.Credit.ToString("N2"), item.Balance.ToString("N2") };
            for (var index = 0; index < values.Length; index++) row.Cells[index].AddParagraph(values[index]);
        }
        if (statement.Items.Count == 0) { var row = table.AddRow(); row.Cells[0].MergeRight = 7; row.Cells[0].AddParagraph("Sin movimientos en el periodo seleccionado."); }
    }

    private static string Period(AccountStatementDto statement) => $"{statement.StartDate?.ToString("yyyy-MM-dd") ?? "inicio"} a {statement.EndDate?.ToString("yyyy-MM-dd") ?? "hoy"}";
    private static string Amount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Csv(string? value)
    {
        var safe = value ?? string.Empty;
        if (safe.TrimStart().StartsWith('=') || safe.TrimStart().StartsWith('+') || safe.TrimStart().StartsWith('-') || safe.TrimStart().StartsWith('@')) safe = $"'{safe}";
        return $"\"{safe.Replace("\"", "\"\"")}\"";
    }
    private static void EnsureFontResolver()
    {
        if (GlobalFontSettings.FontResolver is not null) return;
        lock (FontLock) if (GlobalFontSettings.FontResolver is null) GlobalFontSettings.FontResolver = new AegiFinanceFontResolver();
    }
}

internal sealed class AegiFinanceFontResolver : IFontResolver
{
    public const string FamilyName = "AegiFinance Sans";
    private const string Regular = "AegiFinanceSans-Regular";
    private const string Bold = "AegiFinanceSans-Bold";
    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) => new(isBold ? Bold : Regular);
    public byte[] GetFont(string faceName) => File.ReadAllBytes(faceName == Bold ? BoldPath() : RegularPath());
    private static string RegularPath() => ResolvePath("DejaVuSans.ttf", "arial.ttf");
    private static string BoldPath() => ResolvePath("DejaVuSans-Bold.ttf", "arialbd.ttf");
    private static string ResolvePath(string linuxName, string windowsName)
    {
        var candidates = new[]
        {
            Path.Combine("/usr/share/fonts/truetype/dejavu", linuxName),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), windowsName)
        };
        return candidates.FirstOrDefault(File.Exists) ?? throw new InvalidOperationException("No se encontró la fuente requerida para generar el PDF.");
    }
}
