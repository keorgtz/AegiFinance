namespace AegiFinance.Domain.Enums;

public enum OutboxMessageStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    DeadLetter
}
