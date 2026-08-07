namespace OrderFlow.Contracts;

public sealed record NotificationRequested(Guid MessageId, Guid OrderId, Guid CustomerId, string CorrelationId, DateTimeOffset OccurredAt);
