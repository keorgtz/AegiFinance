using AegiFinance.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Data;

public class ApplicationDbContext : AegiFinanceDbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
