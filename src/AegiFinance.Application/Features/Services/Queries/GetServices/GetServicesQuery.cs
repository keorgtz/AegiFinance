using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Services.Queries.GetServices;

public class GetServicesQuery : IRequest<PaginatedList<ServiceListDto>>
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public BillingType? BillingType { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
