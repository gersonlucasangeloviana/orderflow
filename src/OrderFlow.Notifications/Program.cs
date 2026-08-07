using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using OrderFlow.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IEmailSender, SimulatedEmailSender>();
builder.Services.AddHostedService<NotificationWorker>();
await builder.Build().RunAsync();

public interface IEmailSender { Task SendOrderCreatedAsync(NotificationRequested message, CancellationToken cancellationToken); }
public sealed class SimulatedEmailSender(ILogger<SimulatedEmailSender> logger) : IEmailSender { public Task SendOrderCreatedAsync(NotificationRequested message, CancellationToken cancellationToken) { logger.LogInformation("Simulated e-mail sent for order {OrderId}, correlation {CorrelationId}", message.OrderId, message.CorrelationId); return Task.CompletedTask; } }

public sealed class NotificationWorker(IConfiguration configuration, IEmailSender sender, ILogger<NotificationWorker> logger) : BackgroundService
{
    private readonly ConcurrentDictionary<Guid, byte> _processed = new();
    protected override Task ExecuteAsync(CancellationToken token)
    {
        var uri = configuration["RabbitMq:Uri"] ?? "amqp://guest:guest@rabbitmq:5672/";
        var connection = new ConnectionFactory { Uri = new Uri(uri), DispatchConsumersAsync = true }.CreateConnection();
        var channel = connection.CreateModel();
        channel.ExchangeDeclare("orderflow", ExchangeType.Direct, durable: true);
        channel.ExchangeDeclare("orderflow.dlx", ExchangeType.Direct, durable: true);
        channel.QueueDeclare("notifications.dlq", true, false, false); channel.QueueBind("notifications.dlq", "orderflow.dlx", "notifications.requested");
        channel.QueueDeclare("notifications.requested", true, false, false, new Dictionary<string, object> { ["x-dead-letter-exchange"] = "orderflow.dlx" }); channel.QueueBind("notifications.requested", "orderflow", "notifications.requested");
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, delivery) =>
        {
            try
            {
                var message = JsonSerializer.Deserialize<NotificationRequested>(Encoding.UTF8.GetString(delivery.Body.ToArray())) ?? throw new InvalidOperationException("Invalid notification payload");
                if (_processed.TryAdd(message.MessageId, 0)) await sender.SendOrderCreatedAsync(message, token);
                channel.BasicAck(delivery.DeliveryTag, false);
            }
            catch (Exception exception)
            {
                var retry = delivery.BasicProperties.Headers?.TryGetValue("x-retry-count", out var value) == true ? Convert.ToInt32(value) : 0;
                logger.LogWarning(exception, "Notification failed on attempt {Attempt}", retry + 1);
                if (retry >= 2) channel.BasicNack(delivery.DeliveryTag, false, false);
                else { var properties = channel.CreateBasicProperties(); properties.Persistent = true; properties.Headers = new Dictionary<string, object> { ["x-retry-count"] = retry + 1 }; channel.BasicPublish("orderflow", "notifications.requested", properties, delivery.Body); channel.BasicAck(delivery.DeliveryTag, false); }
            }
        };
        channel.BasicConsume("notifications.requested", false, consumer);
        token.Register(() => { channel.Close(); connection.Close(); channel.Dispose(); connection.Dispose(); });
        return Task.Delay(Timeout.Infinite, token);
    }
}
