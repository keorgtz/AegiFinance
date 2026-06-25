namespace AegiFinance.Application.Dtos;

public class SubscriptionPermissionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid SubscriptionId { get; set; }
    public string SubscriptionCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
}
