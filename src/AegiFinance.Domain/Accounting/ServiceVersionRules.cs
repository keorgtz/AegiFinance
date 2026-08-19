using AegiFinance.Domain.Entities;

namespace AegiFinance.Domain.Accounting;

public static class ServiceVersionRules
{
    public static ServiceVersion? ResolveApplicable(IEnumerable<ServiceVersion> versions, DateTime startDate)
    {
        var businessDate = startDate.Date;

        return versions
            .Where(version => version.IsPublished && version.EffectiveFrom.Date <= businessDate)
            .OrderByDescending(version => version.EffectiveFrom)
            .FirstOrDefault();
    }
}
