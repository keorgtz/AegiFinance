using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Tracker.Queries;

public class GetMonthlyTrackerQuery : IRequest<List<ClientTrackerModel>>
{
    public Guid BankAccountId { get; set; }
    public int Year { get; set; }
}

public class ClientTrackerModel
{
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ClientCode { get; set; } = string.Empty;
    public List<MonthlyStatus> Months { get; set; } = new();
}

public class MonthlyStatus
{
    public int Month { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalPaid { get; set; }
    public BillingItemStatus Status { get; set; }
}

public class GetMonthlyTrackerQueryHandler : IRequestHandler<GetMonthlyTrackerQuery, List<ClientTrackerModel>>
{
    private readonly IApplicationDbContext _context;

    public GetMonthlyTrackerQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientTrackerModel>> Handle(GetMonthlyTrackerQuery request, CancellationToken cancellationToken)
    {
        // 1. Get all billing items for the specified year and group by client
        var billingItems = await _context.BillingItems
            .Include(b => b.Client)
            .Where(b => b.DueDate.Year == request.Year)
            .ToListAsync(cancellationToken);
            
        var clients = billingItems.Select(b => b.Client).DistinctBy(c => c.Id).ToList();

        var result = new List<ClientTrackerModel>();

        foreach (var client in clients)
        {
            var clientModel = new ClientTrackerModel
            {
                ClientId = client.Id,
                ClientName = client.Name,
                ClientCode = client.Code
            };

            var clientBills = billingItems.Where(b => b.ClientId == client.Id).ToList();

            for (int month = 1; month <= 12; month++)
            {
                var monthBills = clientBills.Where(b => b.DueDate.Month == month).ToList();
                var totalBilled = monthBills.Sum(b => b.Amount);
                var totalPaid = monthBills.Sum(b => b.PaidAmount);

                BillingItemStatus aggregateStatus;
                
                if (!monthBills.Any())
                {
                    aggregateStatus = BillingItemStatus.Paid; // No bills means "Paid/Good standing" visually for this month
                }
                else if (monthBills.All(b => b.Status == BillingItemStatus.Paid))
                {
                    aggregateStatus = BillingItemStatus.Paid;
                }
                else if (monthBills.Any(b => b.Status == BillingItemStatus.Partial))
                {
                    aggregateStatus = BillingItemStatus.Partial;
                }
                else
                {
                    aggregateStatus = BillingItemStatus.Pending;
                }

                clientModel.Months.Add(new MonthlyStatus
                {
                    Month = month,
                    TotalBilled = totalBilled,
                    TotalPaid = totalPaid,
                    Status = aggregateStatus
                });
            }

            result.Add(clientModel);
        }

        return result.OrderBy(r => r.ClientName).ToList();
    }
}
