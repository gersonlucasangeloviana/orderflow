using OrderFlow.Contracts;
using OrderFlow.Domain;

namespace OrderFlow.Application;

public sealed class CreateOrder(IOrderRepository orders, INotificationOutbox outbox)
{
    public async Task<Order> ExecuteAsync(Guid customerId, IEnumerable<(Product Product, int Quantity)> lines, decimal freight, string correlationId, CancellationToken cancellationToken)
    {
        var order = new Order();
        foreach (var (product, quantity) in lines) order.AddItem(product, quantity);
        order.SetFreight(freight);
        // A outbox compartilha a transação do pedido; a publicação pode ser retomada com segurança.
        await orders.AddAsync(order, cancellationToken);
        await outbox.EnqueueAsync(new NotificationRequested(Guid.NewGuid(), order.Id, customerId, correlationId, DateTimeOffset.UtcNow), cancellationToken);
        return order;
    }
}
