using System.Text.Json;
using System.Threading.Channels;
using RabbitMQ.Client;
using Serilog.Core;
using Serilog.Events;

public sealed record TechnicalLogEntry(DateTimeOffset Timestamp, string Level, string Service, string MessageTemplate, string Message, string? CorrelationId, string? TraceId, string? UserId, string? OrderId, string? Exception, Dictionary<string, object?> Properties);

public sealed class AsyncRabbitLogSink(ChannelWriter<TechnicalLogEntry> writer) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var properties = logEvent.Properties.Where(pair => !IsSensitive(pair.Key)).ToDictionary(pair => pair.Key, pair => (object?)pair.Value.ToString());
        var entry = new TechnicalLogEntry(logEvent.Timestamp, logEvent.Level.ToString(), "OrderFlow.Api", logEvent.MessageTemplate.Text, logEvent.RenderMessage(), GetProperty(logEvent, "CorrelationId"), null, GetProperty(logEvent, "UserId"), GetProperty(logEvent, "OrderId"), logEvent.Exception?.ToString(), properties);
        // Sob pressão, descarta apenas logs de menor prioridade; erros continuam no console.
        if (!writer.TryWrite(entry) && logEvent.Level >= LogEventLevel.Warning) Console.Error.WriteLine(JsonSerializer.Serialize(entry));
    }
    private static string? GetProperty(LogEvent logEvent, string name) => logEvent.Properties.TryGetValue(name, out var value) ? value.ToString().Trim('"') : null;
    private static bool IsSensitive(string name) => name.Contains("password", StringComparison.OrdinalIgnoreCase) || name.Contains("token", StringComparison.OrdinalIgnoreCase) || name.Contains("connection", StringComparison.OrdinalIgnoreCase);
}

public sealed class RabbitLogPublisher(ChannelReader<TechnicalLogEntry> reader, IConnection connection, ILogger<RabbitLogPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var channel = connection.CreateModel(); channel.ExchangeDeclare("orderflow.logs", ExchangeType.Fanout, durable: true);
        await foreach (var entry in reader.ReadAllAsync(stoppingToken))
        {
            try { var properties = channel.CreateBasicProperties(); properties.Persistent = true; channel.BasicPublish("orderflow.logs", string.Empty, properties, JsonSerializer.SerializeToUtf8Bytes(entry)); }
            catch (Exception exception) { logger.LogError(exception, "Log pipeline publish failed"); }
        }
    }
}
