using AegiFinance.Application.Common.Models;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Enums;
using MediatR;

namespace AegiFinance.Application.Features.Users.Queries.GetUsers;

public class GetUsersQuery : IRequest<PaginatedList<UserDto>>
{
    public string? SearchTerm { get; set; }
    public UserType? UserType { get; set; }
    public Guid? ClientId { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
