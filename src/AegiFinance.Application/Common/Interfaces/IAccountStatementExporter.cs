using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Interfaces;

public interface IAccountStatementExporter
{
    byte[] CreatePdf(AccountStatementDto statement);
    byte[] CreateCsv(AccountStatementDto statement);
}
