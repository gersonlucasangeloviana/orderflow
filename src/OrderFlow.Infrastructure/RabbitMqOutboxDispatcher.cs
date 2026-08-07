using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure;

public interface IOutboxDispatcher { Task DispatchPendingAsync(CancellationToken cancellationToken); }

public sealed class RabbitMqOutboxDispatcher(OrderFlowDbContext db, IConnection connection) : IOutboxDispatcher
{
    public async Task DispatchPendingAsync(CancellationToken cancellationToken)
    {
        var pending = await db.OutboxMessages.Where(message => message.ProcessedAt == null).OrderBy(message => message.OccurredAt).Take(50).ToListAsync(cancellationToken);
        if (pending.Count == 0) return;
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("orderflow", ExchangeType.Direct, durable: true);
        channel.QueueDeclare("notifications.requested", durable: true, exclusive: false, autoDelete: false, arguments: new Dictionary<string, object> { ["x-dead-letter-exchange"] = "orderflow.dlx" });
        channel.QueueBind("notifications.requested", "orderflow", "notifications.requested");
        foreach (var message in pending)
        {
            var properties = channel.CreateBasicProperties(); properties.Persistent = true; properties.MessageId = message.Id.ToString(); properties.Type = message.Type;
            channel.BasicPublish("orderflow", "notifications.requested", properties, Encoding.UTF8.GetBytes(message.Payload));
            message.ProcessedAt = DateTimeOffset.UtcNow; message.Attempts++;
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
