using AegiFinance.Application.Common.Extensions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Services.Commands.CreateServiceVersion;

public sealed class CreateServiceVersionCommandHandler : IRequestHandler<CreateServiceVersionCommand, ServiceVersionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    public CreateServiceVersionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser) { _context = context; _currentUser = currentUser; }

    public async Task<ServiceVersionDto> Handle(CreateServiceVersionCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.IsClientUser()) throw new UnauthorizedAccessException("No tiene permiso para versionar planes.");
        var service = await _context.Services.Include(item => item.Versions).ThenInclude(item => item.Concepts)
            .FirstOrDefaultAsync(item => item.Id == request.ServiceId, cancellationToken)
            ?? throw new InvalidOperationException("El plan no existe.");
        if (service.Versions.Any(item => item.EffectiveFrom == request.EffectiveFrom && item.IsPublished == request.IsPublished))
            throw new InvalidOperationException("Ya existe una versión con la misma fecha efectiva.");

        var version = new ServiceVersion
        {
            Id = Guid.NewGuid(), ServiceId = service.Id, Service = service,
            VersionNumber = service.Versions.Select(item => item.VersionNumber).DefaultIfEmpty().Max() + 1,
            Name = request.Name.Trim(), Description = request.Description, BillingType = request.BillingType,
            BasePrice = request.BasePrice, Currency = request.Currency.Trim().ToUpperInvariant(),
            DefaultDiscountPercent = request.DefaultDiscountPercent, DefaultTaxPercent = request.DefaultTaxPercent,
            CustomIntervalDays = request.CustomIntervalDays, ProrationPolicy = request.ProrationPolicy,
            EffectiveFrom = request.EffectiveFrom, Terms = request.Terms, IsPublished = request.IsPublished
        };
        foreach (var input in request.Concepts.OrderBy(item => item.SortOrder))
            version.Concepts.Add(new ServiceVersionConcept { Id = Guid.NewGuid(), ServiceVersion = version, ServiceVersionId = version.Id, Code = input.Code.Trim().ToUpperInvariant(), Name = input.Name.Trim(), Description = input.Description, Quantity = input.Quantity, UnitPrice = input.UnitPrice, TaxPercent = input.TaxPercent, SortOrder = input.SortOrder });
        service.Versions.Add(version);

        if (version.IsPublished)
        {
            var published = service.Versions.Where(item => item.IsPublished).OrderBy(item => item.EffectiveFrom).ToList();
            for (var index = 0; index < published.Count; index++) published[index].EffectiveTo = index + 1 < published.Count ? published[index + 1].EffectiveFrom : null;
            var current = published.LastOrDefault(item => item.EffectiveFrom <= DateTime.UtcNow);
            if (current is not null) { service.Name = current.Name; service.Description = current.Description; service.BillingType = current.BillingType; service.DefaultPrice = current.BasePrice; service.Currency = current.Currency; }
        }
        await _context.SaveChangesAsync(cancellationToken);
        return Map(version);
    }

    internal static ServiceVersionDto Map(ServiceVersion item) => new(item.Id, item.ServiceId, item.VersionNumber, item.Name, item.Description,
        item.BillingType.ToString(), item.BasePrice, item.Currency, item.DefaultDiscountPercent, item.DefaultTaxPercent,
        item.CustomIntervalDays, item.ProrationPolicy.ToString(), item.EffectiveFrom, item.EffectiveTo, item.Terms, item.IsPublished,
        item.Concepts.OrderBy(concept => concept.SortOrder).Select(concept => new ServiceVersionConceptDto(concept.Id, concept.Code, concept.Name, concept.Description, concept.Quantity, concept.UnitPrice, concept.TaxPercent, concept.SortOrder)).ToList());
}
