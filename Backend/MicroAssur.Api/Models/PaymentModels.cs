namespace MicroAssur.Api.Models;

public enum PaymentProvider
{
    Mtn,
    Orange
}

public enum PaymentPurpose
{
    Subscription,
    Installment
}

public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed
}

public enum SimulationOutcome
{
    Pending,
    Succeeded,
    Failed
}

public sealed record PaymentRecord(
    Guid Id,
    string IdempotencyKey,
    PaymentProvider Provider,
    PaymentPurpose Purpose,
    PaymentStatus Status,
    int Amount,
    string Currency,
    string PhoneNumber,
    string CustomerName,
    DateOnly? BirthDate,
    string? Address,
    string? CategoryId,
    string? CategoryName,
    string? PlanName,
    Guid? SubscriptionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SubscriptionRecord(
    Guid Id,
    Guid InitialPaymentId,
    string CustomerName,
    string PhoneNumber,
    DateOnly BirthDate,
    string Address,
    string CategoryId,
    string CategoryName,
    string PlanName,
    int MonthlyAmount,
    DateTimeOffset CreatedAt);

public sealed record PaymentPlan(string CategoryId, string CategoryName, string PlanName, int MonthlyAmount);
