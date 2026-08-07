using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application;
using OrderFlow.Contracts.Freight;
using RabbitMQ.Client;

namespace OrderFlow.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderFlowInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderFlowDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("SqlServer")));
        services.AddIdentityCore<ApplicationUser>(options => { options.Password.RequiredLength = 12; options.Password.RequireNonAlphanumeric = true; options.User.RequireUniqueEmail = true; })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<OrderFlowDbContext>();
        services.AddScoped<IOrderRepository, SqlOrderRepository>();
        services.AddScoped<INotificationOutbox, SqlNotificationOutbox>();
        services.AddScoped<SqlProductCatalog>();
        var freightAddress = configuration["Freight:Address"] ?? "http://freight:8080";
        services.AddGrpcClient<FreightService.FreightServiceClient>(options => options.Address = new Uri(freightAddress));
        services.AddScoped<IFreightQuoteProvider, GrpcFreightQuoteProvider>();
        var rabbitUri = configuration["RabbitMq:Uri"] ?? "amqp://guest:guest@rabbitmq:5672/";
        services.AddSingleton<IConnection>(_ => new ConnectionFactory { Uri = new Uri(rabbitUri), DispatchConsumersAsync = true }.CreateConnection());
        services.AddScoped<IOutboxDispatcher, RabbitMqOutboxDispatcher>();
        return services;
    }
}
