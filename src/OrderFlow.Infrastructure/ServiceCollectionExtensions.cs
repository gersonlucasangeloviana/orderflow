using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Application;

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
        return services;
    }
}
