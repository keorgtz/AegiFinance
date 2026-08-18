using AegiFinance.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Infrastructure.Data;

public class ApplicationDbContext : AegiFinanceDbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    protected override bool IsClientScope => _currentUserService?.UserType == "Client";
    protected override Guid? CurrentClientId => _currentUserService?.ClientId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}
