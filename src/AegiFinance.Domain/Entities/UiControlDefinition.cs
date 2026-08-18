namespace AegiFinance.Domain.Entities;

public class UiControlDefinition : BaseEntity
{
    public string ControlKey { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string ControlType { get; set; } = string.Empty;
    public string? RequiredPermissionCode { get; set; }
    public bool IsSystemRequired { get; set; }
    public bool IsActive { get; set; } = true;

    public List<UiControlPolicy> Policies { get; set; } = new();
}
