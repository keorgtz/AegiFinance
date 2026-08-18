namespace AegiFinance.Application.Dtos;

public sealed record ClientDocumentDto(Guid Id, Guid ClientId, string Name, string ContentType, long SizeBytes, string? Description, DateTime CreatedAt);
public sealed record ClientTimelineItemDto(string Id, DateTime OccurredAt, string Type, string Title, string? Detail, Guid? ActorUserId);
public sealed record ClientDuplicateRuleDto(bool MatchTaxId, bool MatchName, bool MatchBillingEmail, bool BlockOnMatch);
public sealed record ClientDuplicateMatchDto(Guid Id, string Code, string Name, IReadOnlyList<string> MatchedFields);
