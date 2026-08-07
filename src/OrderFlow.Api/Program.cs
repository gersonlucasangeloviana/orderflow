using OrderFlow.Application;
using OrderFlow.Domain;
using OrderFlow.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddRateLimiter(options => options.AddFixedWindowLimiter("public", o => { o.PermitLimit = 100; o.Window = TimeSpan.FromMinutes(1); }));
builder.Services.AddOrderFlowInfrastructure(builder.Configuration);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key must be provided through the Jwt__Key environment variable.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "OrderFlow", ValidateAudience = true, ValidAudience = builder.Configuration["Jwt:Audience"] ?? "OrderFlow", ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), ValidateLifetime = true });
builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireRole("Admin")));
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddHostedService<OutboxPublisherWorker>();
builder.Services.AddScoped<CreateOrder>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderFlowDbContext>();
    await db.Database.MigrateAsync();
    var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var role in new[] { "Admin", "Customer" }) if (!await roles.RoleExistsAsync(role)) await roles.CreateAsync(new IdentityRole<Guid>(role));
}
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger(); app.UseSwaggerUI();
app.Use(async (context, next) => { context.Response.Headers.TryAdd("X-Correlation-Id", context.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString()); await next(); });
app.MapHealthChecks("/health");
app.MapPost("/api/auth/register", async (RegisterRequest request, UserManager<ApplicationUser> users, CancellationToken cancellationToken) =>
{
    var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = request.Email, Email = request.Email, DisplayName = request.Name };
    var result = await users.CreateAsync(user, request.Password);
    if (!result.Succeeded) return Results.ValidationProblem(result.Errors.GroupBy(error => error.Code).ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray()));
    await users.AddToRoleAsync(user, "Customer");
    return Results.Created($"/api/users/{user.Id}", new { user.Id, user.Email });
});
app.MapPost("/api/auth/login", async (LoginRequest request, UserManager<ApplicationUser> users, JwtTokenService tokens) =>
{
    var user = await users.FindByEmailAsync(request.Email);
    if (user is null || !await users.CheckPasswordAsync(user, request.Password)) return Results.Unauthorized();
    return Results.Ok(new { accessToken = await tokens.CreateAsync(user) });
});
app.MapGet("/api/products", async (SqlProductCatalog catalog, CancellationToken cancellationToken) => Results.Ok(await catalog.ListAsync(cancellationToken))).RequireRateLimiting("public");
app.MapPost("/api/freight/quote", async (FreightQuoteRequest request, IFreightQuoteProvider freight, CancellationToken cancellationToken) =>
{
    try { return Results.Ok(new { amount = await freight.CalculateAsync(request.PostalCode, request.TotalWeight, request.CartValue, cancellationToken) }); }
    catch (FreightUnavailableException) { return Results.Problem("O frete está temporariamente indisponível.", statusCode: StatusCodes.Status503ServiceUnavailable); }
}).RequireAuthorization();
app.MapPost("/api/orders", async (CreateOrderRequest request, CreateOrder useCase, OrderFlowDbContext db, HttpContext context, CancellationToken cancellationToken) =>
{
    var ids = request.Items.Select(item => item.ProductId).Distinct().ToArray();
    var products = await db.Products.Where(product => ids.Contains(product.Id) && product.IsActive).ToDictionaryAsync(product => product.Id, cancellationToken);
    if (products.Count != ids.Length) return Results.ValidationProblem(new Dictionary<string, string[]> { ["items"] = ["Um ou mais produtos não foram encontrados."] });
    var lines = request.Items.Select(item => (new Product(products[item.ProductId].Id, products[item.ProductId].Name, products[item.ProductId].Sku, products[item.ProductId].Price), item.Quantity));
    var customerId = Guid.TryParse(context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : Guid.Empty;
    if (customerId == Guid.Empty) return Results.Unauthorized();
    var order = await useCase.ExecuteAsync(customerId, lines, request.Freight, context.Response.Headers["X-Correlation-Id"].ToString(), cancellationToken);
    return Results.Created($"/api/orders/{order.Id}", new { order.Id, order.Total, order.Status });
}).RequireAuthorization();
app.Run();

public sealed record CreateOrderRequest(decimal Freight, List<CreateOrderItem> Items);
public sealed record CreateOrderItem(Guid ProductId, int Quantity);
public sealed record FreightQuoteRequest(string PostalCode, decimal TotalWeight, decimal CartValue);
public sealed record RegisterRequest(string Name, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
