using OrderFlow.Contracts;
using OrderFlow.Domain;

namespace OrderFlow.Application;

public interface IOrderRepository { Task AddAsync(Order order, CancellationToken cancellationToken); }
public interface INotificationOutbox { Task EnqueueAsync(NotificationRequested message, CancellationToken cancellationToken); }
public interface IFreightQuoteProvider { Task<decimal> CalculateAsync(string postalCode, decimal weight, decimal cartValue, CancellationToken cancellationToken); }
