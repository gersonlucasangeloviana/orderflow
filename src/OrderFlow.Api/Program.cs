using OrderFlow.Application;
using OrderFlow.Domain;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("public", o => { o.PermitLimit = 100; o.Window = TimeSpan.FromMinutes(1); }));
builder.Services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
builder.Services.AddSingleton<INotificationOutbox, InMemoryOutbox>();
builder.Services.AddScoped<CreateOrder>();
var app = builder.Build();
app.UseRateLimiter();
app.UseSwagger(); app.UseSwaggerUI();
app.Use(async (context, next) => { context.Response.Headers.TryAdd("X-Correlation-Id", context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString()); await next(); });
app.MapHealthChecks("/health");
app.MapGet("/api/products", () => Results.Ok(new[] { new Product(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Notebook", "NB-1", 2999m) })).RequireRateLimiting("public");
app.MapPost("/api/orders", async (CreateOrderRequest request, CreateOrder useCase, HttpContext context, CancellationToken cancellationToken) =>
{
    var product = new Product(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Notebook", "NB-1", 2999m);
    var order = await useCase.ExecuteAsync(request.CustomerId, request.Items.Select(i => (product, i.Quantity)), request.Freight, context.Response.Headers["X-Correlation-Id"].ToString(), cancellationToken);
    return Results.Created($"/api/orders/{order.Id}", new { order.Id, order.Total, order.Status });
});
app.Run();

public sealed record CreateOrderRequest(Guid CustomerId, decimal Freight, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(int Quantity);
sealed class InMemoryOrderRepository : IOrderRepository { public Task AddAsync(Order order, CancellationToken cancellationToken) => Task.CompletedTask; }
sealed class InMemoryOutbox : INotificationOutbox { public Task EnqueueAsync(OrderFlow.Contracts.NotificationRequested message, CancellationToken cancellationToken) => Task.CompletedTask; }
