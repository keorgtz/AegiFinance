using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.CreateClient;

public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ClientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClientCodeGenerator _codeGenerator;

    public CreateClientCommandHandler(IApplicationDbContext context, IClientCodeGenerator codeGenerator)
    {
        _context = context;
        _codeGenerator = codeGenerator;
    }

    public async Task<ClientDto> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        ClientCategory? category = null;
        if (request.CategoryId.HasValue)
        {
            category = await _context.ClientCategories
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (category is null)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }
        }

        List<ClientTag> tags = new();
        if (request.TagIds?.Count > 0)
        {
            tags = await _context.ClientTags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (tags.Count != request.TagIds.Distinct().Count())
            {
                throw new InvalidOperationException("Una o más etiquetas no existen.");
            }
        }

        var code = await _codeGenerator.GenerateAsync(cancellationToken);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = request.Name,
            TradeName = request.TradeName,
            TaxId = request.TaxId,
            BillingEmail = request.BillingEmail,
            BillingAddress = request.BillingAddress,
            Phone = request.Phone,
            Status = request.Status,
            Notes = request.Notes,
            CategoryId = request.CategoryId,
            Category = category,
            Tags = tags
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(client);
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            Code = client.Code,
            Name = client.Name,
            TradeName = client.TradeName,
            TaxId = client.TaxId,
            BillingEmail = client.BillingEmail,
            BillingAddress = client.BillingAddress,
            Phone = client.Phone,
            Status = client.Status.ToString(),
            Notes = client.Notes,
            CategoryId = client.CategoryId,
            CategoryName = client.Category?.Name,
            Tags = client.Tags.Select(t => new ClientTagDto
            {
                Id = t.Id,
                Name = t.Name,
                Color = t.Color
            }).ToList()
        };
    }
}
