using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Clients.Queries.GetClients;

public class GetClientsQuery : IRequest<PaginatedList<ClientListDto>>
{
    public string? SearchTerm { get; set; }
    public ClientStatus? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? TagId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
