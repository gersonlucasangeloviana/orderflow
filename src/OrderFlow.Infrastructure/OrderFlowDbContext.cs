using Microsoft.EntityFrameworkCore;

namespace OrderFlow.Infrastructure;

public sealed class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<OrderItemEntity> OrderItems => Set<OrderItemEntity>();
    public DbSet<OutboxMessageEntity> OutboxMessages => Set<OutboxMessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductEntity>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => x.Sku).IsUnique(); entity.Property(x => x.Name).HasMaxLength(160).IsRequired(); entity.Property(x => x.Price).HasPrecision(18, 2); });
        modelBuilder.Entity<OrderEntity>(entity => { entity.HasKey(x => x.Id); entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(24); entity.Property(x => x.Freight).HasPrecision(18, 2); entity.Property(x => x.Total).HasPrecision(18, 2); entity.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade); });
        modelBuilder.Entity<OrderItemEntity>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => x.OrderId); entity.Property(x => x.ProductName).HasMaxLength(160); entity.Property(x => x.Sku).HasMaxLength(80); entity.Property(x => x.UnitPrice).HasPrecision(18, 2); });
        modelBuilder.Entity<OutboxMessageEntity>(entity => { entity.HasKey(x => x.Id); entity.HasIndex(x => new { x.ProcessedAt, x.OccurredAt }); entity.Property(x => x.Type).HasMaxLength(200); entity.Property(x => x.Payload).IsRequired(); });
    }
}

public sealed class ProductEntity { public Guid Id { get; set; } public required string Name { get; set; } public required string Sku { get; set; } public decimal Price { get; set; } public bool IsActive { get; set; } = true; }
public sealed class OrderEntity { public Guid Id { get; set; } public Guid CustomerId { get; set; } public string Status { get; set; } = "Created"; public decimal Freight { get; set; } public decimal Total { get; set; } public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; public List<OrderItemEntity> Items { get; set; } = []; }
public sealed class OrderItemEntity { public Guid Id { get; set; } public Guid OrderId { get; set; } public Guid ProductId { get; set; } public required string ProductName { get; set; } public required string Sku { get; set; } public decimal UnitPrice { get; set; } public int Quantity { get; set; } }
public sealed class OutboxMessageEntity { public Guid Id { get; set; } public required string Type { get; set; } public required string Payload { get; set; } public DateTimeOffset OccurredAt { get; set; } public DateTimeOffset? ProcessedAt { get; set; } public int Attempts { get; set; } }
