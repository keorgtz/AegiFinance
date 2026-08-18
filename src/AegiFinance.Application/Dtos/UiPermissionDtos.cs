namespace AegiFinance.Application.Dtos;

public sealed class UiControlDefinitionDto
{
    public string ControlKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ControlType { get; set; } = string.Empty;
    public string? RequiredPermissionCode { get; set; }
    public bool IsSystemRequired { get; set; }
}

public sealed class UiControlPolicyDto
{
    public Guid UiControlDefinitionId { get; set; }
    public Guid? RoleId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public string AccessMode { get; set; } = "Enabled";
    public DateTime? ExpiresAt { get; set; }
}

public sealed class UiPermissionSimulationRequest
{
    public Guid RoleId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
}
