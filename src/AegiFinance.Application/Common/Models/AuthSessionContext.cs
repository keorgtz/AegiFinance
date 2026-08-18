namespace AegiFinance.Application.Common.Models;

public sealed record AuthSessionContext(string? IpAddress, string? UserAgent, string? DeviceName);
