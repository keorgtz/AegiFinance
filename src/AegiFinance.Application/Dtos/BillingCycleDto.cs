namespace AegiFinance.Application.Dtos;

public class BillingCycleDto
{
    public Guid Id { get; set; }
    public int Year { get; set; }
    public int? Month { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ClosedAt { get; set; }
    public Guid? ClosedBy { get; set; }
    public int ItemsCount { get; set; }
}
