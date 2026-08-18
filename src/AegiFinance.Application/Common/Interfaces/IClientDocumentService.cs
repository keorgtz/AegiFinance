using AegiFinance.Application.Dtos;

namespace AegiFinance.Application.Common.Interfaces;

public interface IClientDocumentService
{
    Task<ClientDocumentDto> SaveAsync(Guid clientId, string fileName, string contentType, long sizeBytes, string? description, Stream content, CancellationToken cancellationToken = default);
    Task<ClientDocumentDownload> OpenAsync(Guid clientId, Guid documentId, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid clientId, Guid documentId, CancellationToken cancellationToken = default);
}

public sealed record ClientDocumentDownload(Stream Content, string ContentType, string FileName);
