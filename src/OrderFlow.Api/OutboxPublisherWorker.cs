using OrderFlow.Infrastructure;

public sealed class OutboxPublisherWorker(IServiceScopeFactory scopes, ILogger<OutboxPublisherWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { using var scope = scopes.CreateScope(); await scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>().DispatchPendingAsync(stoppingToken); }
            catch (Exception exception) { logger.LogError(exception, "Could not dispatch pending outbox messages"); }
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
