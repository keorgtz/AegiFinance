namespace AegiFinance.Application.Common.Models;

public class BillingGenerationResult
{
    public bool Success { get; set; }
    public int ItemsGenerated { get; set; }
    public List<string> Errors { get; set; } = new();
}
