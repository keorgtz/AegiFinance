using System.Text.RegularExpressions;
using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Services;

public class ServiceCodeGenerator : IServiceCodeGenerator
{
    private readonly ApplicationDbContext _context;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public ServiceCodeGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            var lastCode = await _context.Services
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(s => s.Code.StartsWith("SRV-"))
                .OrderByDescending(s => s.Code)
                .Select(s => s.Code)
                .FirstOrDefaultAsync(cancellationToken);

            var nextNumber = 1;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var match = Regex.Match(lastCode, @"SRV-(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"SRV-{nextNumber:D5}";
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
