using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Clients.Queries.GetClientById;

public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetClientByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ClientDetailDto> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        var isClientUser = Enum.TryParse<UserType>(_currentUserService.UserType, out var userType)
            && userType == UserType.Client;

        if (isClientUser && (!_currentUserService.ClientId.HasValue || _currentUserService.ClientId.Value != request.Id))
        {
            throw new InvalidOperationException("No tiene permiso para consultar este cliente.");
        }

        var client = await _context.Clients
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Tags)
            .Include(c => c.Contacts)
            .Include(c => c.NotesList)
            .Include(c => c.Documents)
            .Include(c => c.AccountManagerUser)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("El cliente no existe.");
        }

        return new ClientDetailDto
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
            }).ToList(),
            Contacts = client.Contacts.Select(c => new ClientContactDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                Name = c.Name,
                Email = c.Email,
                Phone = c.Phone,
                Position = c.Position,
                IsPrimary = c.IsPrimary
            }).ToList(),
            NotesList = client.NotesList.Select(n => new ClientNoteDto
            {
                Id = n.Id,
                ClientId = n.ClientId,
                Content = n.Content,
                IsPinned = n.IsPinned,
                CreatedAt = n.CreatedAt
            }).OrderByDescending(n => n.IsPinned).ThenByDescending(n => n.CreatedAt).ToList(),
            Documents = client.Documents.OrderByDescending(document => document.CreatedAt).Select(document =>
                new ClientDocumentDto(document.Id, document.ClientId, document.Name, document.ContentType,
                    document.SizeBytes, document.Description, document.CreatedAt)).ToList(),
            PresentationCurrency = client.PresentationCurrency,
            PaymentTermsDays = client.PaymentTermsDays,
            CreditLimit = client.CreditLimit,
            CommercialTerms = client.CommercialTerms,
            AccountManagerUserId = client.AccountManagerUserId,
            AccountManagerName = client.AccountManagerUser?.Name
        };
    }
}
