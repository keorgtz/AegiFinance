using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/clients/{clientId:guid}/documents")]
[Authorize]
public sealed class ClientDocumentsController : ControllerBase
{
    private readonly IClientDocumentService _documents;
    public ClientDocumentsController(IClientDocumentService documents) => _documents = documents;

    [HttpPost]
    [Authorize(Policy = "ManageClientDocuments")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ClientDocumentDto>> Upload(Guid clientId, IFormFile file, [FromForm] string? description, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await _documents.SaveAsync(clientId, file.FileName, file.ContentType, file.Length, description, stream, cancellationToken));
    }

    [HttpGet("{documentId:guid}")]
    [Authorize(Policy = "ViewClientDocuments")]
    public async Task<IActionResult> Download(Guid clientId, Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documents.OpenAsync(clientId, documentId, cancellationToken);
        return File(result.Content, result.ContentType, result.FileName);
    }

    [HttpDelete("{documentId:guid}")]
    [Authorize(Policy = "ManageClientDocuments")]
    public async Task<IActionResult> Delete(Guid clientId, Guid documentId, CancellationToken cancellationToken)
    {
        await _documents.DeleteAsync(clientId, documentId, cancellationToken);
        return NoContent();
    }
}
