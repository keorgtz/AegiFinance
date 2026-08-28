using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Interfaces;

public interface IFinancialReportingService
{
    Task<FinancialReportDto> GenerateAsync(FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    Task<FinancialReportDto> GenerateForOrganizationAsync(Guid organizationId, FinancialReportQueryDto query, CancellationToken cancellationToken = default);
    byte[] CreateCsv(FinancialReportDto report);
    Task<IReadOnlyList<ReportAccountDto>> GetAccountsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReportScheduleDto>> GetSchedulesAsync(CancellationToken cancellationToken = default);
    Task<ReportScheduleDto> SaveScheduleAsync(Guid? id, SaveReportScheduleRequest request, CancellationToken cancellationToken = default);
    Task DeleteScheduleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReportRunDto>> GetRunsAsync(CancellationToken cancellationToken = default);
    Task<ReportFileDto> GetRunFileAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IReportScheduleProcessor
{
    Task<int> ProcessDueAsync(CancellationToken cancellationToken = default);
}
