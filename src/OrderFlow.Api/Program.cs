using OrderFlow.Application;
using OrderFlow.Domain;
using OrderFlow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("public", o => { o.PermitLimit = 100; o.Window = TimeSpan.FromMinutes(1); }));
builder.Services.AddOrderFlowInfrastructure(builder.Configuration);
builder.Services.AddScoped<CreateOrder>();
var app = builder.Build();
app.UseRateLimiter();
app.UseSwagger(); app.UseSwaggerUI();
app.Use(async (context, next) => { context.Response.Headers.TryAdd("X-Correlation-Id", context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString()); await next(); });
app.MapHealthChecks("/health");
app.MapGet("/api/products", async (SqlProductCatalog catalog, CancellationToken cancellationToken) => Results.Ok(await catalog.ListAsync(cancellationToken))).RequireRateLimiting("public");
app.MapPost("/api/orders", async (CreateOrderRequest request, CreateOrder useCase, OrderFlowDbContext db, HttpContext context, CancellationToken cancellationToken) =>
{
    var ids = request.Items.Select(item => item.ProductId).Distinct().ToArray();
    var products = await db.Products.Where(product => ids.Contains(product.Id) && product.IsActive).ToDictionaryAsync(product => product.Id, cancellationToken);
    if (products.Count != ids.Length) return Results.ValidationProblem(new Dictionary<string, string[]> { ["items"] = ["Um ou mais produtos não foram encontrados."] });
    var lines = request.Items.Select(item => (new Product(products[item.ProductId].Id, products[item.ProductId].Name, products[item.ProductId].Sku, products[item.ProductId].Price), item.Quantity));
    var order = await useCase.ExecuteAsync(request.CustomerId, lines, request.Freight, context.Response.Headers["X-Correlation-Id"].ToString(), cancellationToken);
    return Results.Created($"/api/orders/{order.Id}", new { order.Id, order.Total, order.Status });
});
app.Run();

public sealed record CreateOrderRequest(Guid CustomerId, decimal Freight, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(Guid ProductId, int Quantity);
