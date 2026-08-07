using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OrderFlow.Infrastructure;

public sealed class OrderFlowDbContextFactory : IDesignTimeDbContextFactory<OrderFlowDbContext>
{
    public OrderFlowDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ORDERFLOW_SQL_CONNECTION")
            ?? "Server=localhost;Database=orderflow;Trusted_Connection=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<OrderFlowDbContext>()
            .UseSqlServer(connection)
            .Options;
        return new OrderFlowDbContext(options);
    }
}
