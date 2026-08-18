using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AegiFinance.Infrastructure.Services;

public sealed class ClientDocumentService : IClientDocumentService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".txt" };
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly string _root;

    public ClientDocumentService(ApplicationDbContext context, ICurrentUserService currentUser, IConfiguration configuration)
    {
        _context = context;
        _currentUser = currentUser;
        _root = Path.GetFullPath(configuration["ClientDocuments:Path"] ?? Path.Combine(AppContext.BaseDirectory, "data", "client-documents"));
    }

    public async Task<ClientDocumentDto> SaveAsync(Guid clientId, string fileName, string contentType, long sizeBytes, string? description, Stream content, CancellationToken cancellationToken = default)
    {
        var client = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(item => item.Id == clientId, cancellationToken)
            ?? throw new InvalidOperationException("El cliente no existe o está fuera de su organización.");
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension)) throw new InvalidOperationException("El tipo de archivo no está permitido.");
        if (sizeBytes <= 0 || sizeBytes > 10 * 1024 * 1024) throw new InvalidOperationException("El documento debe pesar entre 1 byte y 10 MB.");

        var safeName = Path.GetFileName(fileName);
        var relative = Path.Combine(client.OrganizationId.ToString("N"), clientId.ToString("N"), $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
        var absolute = Path.GetFullPath(Path.Combine(_root, relative));
        if (!absolute.StartsWith(_root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Ruta de documento inválida.");
        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
        await using (var output = new FileStream(absolute, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            await content.CopyToAsync(output, cancellationToken);

        var document = new ClientDocument { Id = Guid.NewGuid(), ClientId = clientId, Name = safeName,
            StorageKey = relative.Replace('\\', '/'), ContentType = contentType, SizeBytes = sizeBytes, Description = description };
        _context.ClientDocuments.Add(document);
        await _context.SaveChangesAsync(cancellationToken);
        return new ClientDocumentDto(document.Id, clientId, document.Name, document.ContentType, document.SizeBytes, document.Description, document.CreatedAt);
    }

    public async Task<ClientDocumentDownload> OpenAsync(Guid clientId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.ClientDocuments.AsNoTracking().FirstOrDefaultAsync(item => item.Id == documentId && item.ClientId == clientId, cancellationToken)
            ?? throw new KeyNotFoundException("El documento no existe.");
        var absolute = Path.GetFullPath(Path.Combine(_root, document.StorageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!absolute.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(absolute)) throw new KeyNotFoundException("El archivo no está disponible.");
        return new ClientDocumentDownload(new FileStream(absolute, FileMode.Open, FileAccess.Read, FileShare.Read), document.ContentType, document.Name);
    }

    public async Task DeleteAsync(Guid clientId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _context.ClientDocuments.FirstOrDefaultAsync(item => item.Id == documentId && item.ClientId == clientId, cancellationToken)
            ?? throw new KeyNotFoundException("El documento no existe.");
        _context.ClientDocuments.Remove(document);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
