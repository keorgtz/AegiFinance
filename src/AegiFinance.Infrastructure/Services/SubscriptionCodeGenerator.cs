using System.Text.RegularExpressions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class SubscriptionCodeGenerator : ISubscriptionCodeGenerator
{
    private readonly ApplicationDbContext _context;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public SubscriptionCodeGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var lastCode = await _context.Subscriptions
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(s => s.Code.StartsWith("SUB-"))
                .OrderByDescending(s => s.Code)
                .Select(s => s.Code)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var match = Regex.Match(lastCode, @"SUB-(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"SUB-{nextNumber:D5}";
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
