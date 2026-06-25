using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Commands.UpdateClient;

public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateClientCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDto> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        if (_currentUserService.IsClientUser())
        {
            throw new UnauthorizedAccessException("No tiene permiso para modificar clientes.");
        }

        var client = await _context.Clients
            .Include(c => c.Tags)
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _context.ClientCategories
                .AsNoTracking()
                .AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

            if (!categoryExists)
            {
                throw new InvalidOperationException("La categoría seleccionada no existe.");
            }
        }

        client.Name = request.Name;
        client.TradeName = request.TradeName;
        client.TaxId = request.TaxId;
        client.BillingEmail = request.BillingEmail;
        client.BillingAddress = request.BillingAddress;
        client.Phone = request.Phone;
        client.Status = request.Status;
        client.Notes = request.Notes;
        client.CategoryId = request.CategoryId;

        if (request.TagIds is not null)
        {
            var tags = await _context.ClientTags
                .Where(t => request.TagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (tags.Count != request.TagIds.Distinct().Count())
            {
                throw new InvalidOperationException("Una o más etiquetas no existen.");
            }

            client.Tags.Clear();
            foreach (var tag in tags)
            {
                client.Tags.Add(tag);
            }
        }

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
