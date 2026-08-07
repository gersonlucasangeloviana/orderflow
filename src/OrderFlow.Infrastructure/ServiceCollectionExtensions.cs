using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application;

namespace OrderFlow.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrderFlowInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrderFlowDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("SqlServer")));
        services.AddScoped<IOrderRepository, SqlOrderRepository>();
        services.AddScoped<INotificationOutbox, SqlNotificationOutbox>();
        services.AddScoped<SqlProductCatalog>();
        return services;
    }
}
