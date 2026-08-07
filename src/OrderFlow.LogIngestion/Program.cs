using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<LogIngestionWorker>();
await builder.Build().RunAsync();

public sealed class LogIngestionWorker(IConfiguration configuration, ILogger<LogIngestionWorker> logger) : BackgroundService
{
    private readonly ConcurrentQueue<TechnicalLogDocument> _buffer = new();
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        var mongo = new MongoClient(configuration["Mongo:ConnectionString"] ?? "mongodb://mongo:27017").GetDatabase("orderflow").GetCollection<TechnicalLogDocument>("technical_logs");
        await CreateIndexesAsync(mongo, token);
        using var connection = new ConnectionFactory { Uri = new Uri(configuration["RabbitMq:Uri"] ?? "amqp://guest:guest@rabbitmq:5672/"), DispatchConsumersAsync = true }.CreateConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare("orderflow.logs", ExchangeType.Fanout, durable: true);
        channel.ExchangeDeclare("orderflow.logs.dlx", ExchangeType.Fanout, durable: true);
        channel.QueueDeclare("logs.ingestion.dlq", true, false, false); channel.QueueBind("logs.ingestion.dlq", "orderflow.logs.dlx", string.Empty);
        channel.QueueDeclare("logs.ingestion", true, false, false, new Dictionary<string, object> { ["x-dead-letter-exchange"] = "orderflow.logs.dlx" }); channel.QueueBind("logs.ingestion", "orderflow.logs", string.Empty);
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += (_, delivery) => { try { var entry = JsonSerializer.Deserialize<TechnicalLogDocument>(Encoding.UTF8.GetString(delivery.Body.ToArray())); if (entry is not null) _buffer.Enqueue(entry); channel.BasicAck(delivery.DeliveryTag, false); } catch (Exception exception) { logger.LogError(exception, "Invalid technical log event"); channel.BasicNack(delivery.DeliveryTag, false, false); } return Task.CompletedTask; };
        channel.BasicConsume("logs.ingestion", false, consumer);
        while (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), token);
            var batch = new List<TechnicalLogDocument>(); while (batch.Count < 100 && _buffer.TryDequeue(out var entry)) batch.Add(entry);
            if (batch.Count == 0) continue;
            try { await mongo.InsertManyAsync(batch, cancellationToken: token); }
            catch (Exception exception) { logger.LogError(exception, "MongoDB log batch insertion failed; logs remain available in console fallback"); }
        }
    }
    private static async Task CreateIndexesAsync(IMongoCollection<TechnicalLogDocument> logs, CancellationToken token)
    {
        var keys = new[] { "Timestamp", "Level", "Service", "CorrelationId", "OrderId" }.Select(field => new CreateIndexModel<TechnicalLogDocument>(Builders<TechnicalLogDocument>.IndexKeys.Ascending(field))).ToList();
        keys.Add(new CreateIndexModel<TechnicalLogDocument>(Builders<TechnicalLogDocument>.IndexKeys.Ascending(item => item.Timestamp), new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) }));
        await logs.Indexes.CreateManyAsync(keys, token);
    }
}

public sealed class TechnicalLogDocument
{
    [BsonId] public ObjectId Id { get; set; }
    public DateTimeOffset Timestamp { get; set; } public string Level { get; set; } = "Information"; public string Service { get; set; } = ""; public string MessageTemplate { get; set; } = ""; public string Message { get; set; } = ""; public string? CorrelationId { get; set; } public string? TraceId { get; set; } public string? UserId { get; set; } public string? OrderId { get; set; } public string? Exception { get; set; } public Dictionary<string, object?> Properties { get; set; } = [];
}
