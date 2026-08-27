using System.Text.Json;
using AegiFinance.Application.Features.BankImports;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/bank-imports")]
[Authorize]
public sealed class BankImportsController : ControllerBase
{
    private readonly IMediator _mediator;
    public BankImportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("adapters")]
    [Authorize(Policy = "CreateBankStatementImports")]
    public ActionResult<IReadOnlyList<BankImportAdapterDto>> GetAdapters() => Ok(BankImportMapping.Adapters);

    [HttpGet("profiles")]
    [Authorize(Policy = "CreateBankStatementImports")]
    public async Task<ActionResult<IReadOnlyList<BankImportProfileDto>>> GetProfiles(CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetBankImportProfilesQuery(), cancellationToken));

    [HttpPost("profiles")]
    [Authorize(Policy = "ManageBankImportProfiles")]
    public async Task<ActionResult<BankImportProfileDto>> CreateProfile(CreateBankImportProfileCommand command, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(command, cancellationToken));

    [HttpGet]
    [Authorize(Policy = "ViewBankStatementImports")]
    public async Task<ActionResult<IReadOnlyList<BankImportBatchDto>>> List([FromQuery] GetBankImportsQuery query, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(query, cancellationToken));

    [HttpPost("preview")]
    [Authorize(Policy = "CreateBankStatementImports")]
    [RequestSizeLimit(10_600_000)]
    public async Task<ActionResult<BankImportBatchDto>> Preview([FromForm] PreviewBankImportRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null) return BadRequest(new ProblemDetails { Title = "Error de validación", Detail = "Seleccioná un archivo CSV o XLSX." });
        await using var stream = request.File.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        BankImportColumnMap? columns = null;
        if (!string.IsNullOrWhiteSpace(request.ColumnsJson))
        {
            try { columns = JsonSerializer.Deserialize<BankImportColumnMap>(request.ColumnsJson, new JsonSerializerOptions(JsonSerializerDefaults.Web)); }
            catch (JsonException) { return BadRequest(new ProblemDetails { Title = "Error de validación", Detail = "La asignación de columnas no es válida." }); }
        }
        return Ok(await _mediator.Send(new PreviewBankImportCommand
        {
            BankAccountId = request.BankAccountId, Content = memory.ToArray(), FileName = request.File.FileName,
            AdapterCode = string.IsNullOrWhiteSpace(request.AdapterCode) ? "generic" : request.AdapterCode,
            ProfileId = request.ProfileId, Columns = columns
        }, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ViewBankStatementImports")]
    public async Task<ActionResult<BankImportBatchDto>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new GetBankImportQuery(id), cancellationToken));

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Policy = "ConfirmBankStatementImports")]
    public async Task<ActionResult<BankImportBatchDto>> Confirm(Guid id, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new ConfirmBankImportCommand(id), cancellationToken));

    [HttpPost("{id:guid}/rollback")]
    [Authorize(Policy = "RollbackBankStatementImports")]
    public async Task<IActionResult> Rollback(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RollbackBankImportCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed class PreviewBankImportRequest
{
    public Guid BankAccountId { get; set; }
    public IFormFile? File { get; set; }
    public string AdapterCode { get; set; } = "generic";
    public Guid? ProfileId { get; set; }
    public string? ColumnsJson { get; set; }
}
