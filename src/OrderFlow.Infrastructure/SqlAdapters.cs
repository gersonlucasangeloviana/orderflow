using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Application;
using OrderFlow.Contracts;
using OrderFlow.Domain;

namespace OrderFlow.Infrastructure;

public sealed class SqlOrderRepository(OrderFlowDbContext db) : IOrderRepository
{
    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        db.Orders.Add(new OrderEntity { Id = order.Id, CustomerId = order.CustomerId, Status = order.Status.ToString(), Freight = order.Freight, Total = order.Total, Items = order.Items.Select(item => new OrderItemEntity { Id = Guid.NewGuid(), OrderId = order.Id, ProductId = item.ProductId, ProductName = item.ProductName, Sku = item.Sku, UnitPrice = item.UnitPrice, Quantity = item.Quantity }).ToList() });
        return Task.CompletedTask;
    }
}

public sealed class SqlNotificationOutbox(OrderFlowDbContext db) : INotificationOutbox
{
    public async Task EnqueueAsync(NotificationRequested message, CancellationToken cancellationToken)
    {
        db.OutboxMessages.Add(new OutboxMessageEntity { Id = message.MessageId, Type = nameof(NotificationRequested), Payload = JsonSerializer.Serialize(message), OccurredAt = message.OccurredAt });
        // Este único SaveChanges persiste pedido e outbox no mesmo DbContext/commit SQL.
        await db.SaveChangesAsync(cancellationToken);
    }
}

public sealed class SqlProductCatalog(OrderFlowDbContext db)
{
    public Task<List<ProductEntity>> ListAsync(CancellationToken cancellationToken) => db.Products.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync(cancellationToken);
}
