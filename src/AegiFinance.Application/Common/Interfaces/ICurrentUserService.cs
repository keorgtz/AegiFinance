namespace AegiFinance.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    string? UserType { get; }
    Guid? ClientId { get; }
}
