namespace OrderFlow.Domain;

public enum OrderStatus { Created, Confirmed, Shipped, Cancelled }

public sealed class Order
{
    private readonly List<OrderItem> _items = [];
    public Guid Id { get; } = Guid.NewGuid();
    public OrderStatus Status { get; private set; } = OrderStatus.Created;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal Freight { get; private set; }
    public decimal Total => _items.Sum(item => item.UnitPrice * item.Quantity) + Freight;

    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0) throw new DomainException("Quantidade deve ser positiva.");
        // Snapshot preserva o histórico mesmo se o catálogo mudar depois.
        _items.Add(new OrderItem(product.Id, product.Name, product.Sku, product.Price, quantity));
    }

    public void SetFreight(decimal amount)
    {
        if (amount < 0) throw new DomainException("Frete não pode ser negativo.");
        Freight = amount;
    }

    public void Confirm()
    {
        if (_items.Count == 0) throw new DomainException("Pedido deve possuir pelo menos um item.");
        if (Status == OrderStatus.Cancelled) throw new DomainException("Pedido cancelado não pode ser confirmado.");
        if (Status != OrderStatus.Created) throw new DomainException("Transição de pedido inválida.");
        Status = OrderStatus.Confirmed;
    }

    public void Ship()
    {
        if (Status != OrderStatus.Confirmed) throw new DomainException("Somente pedidos confirmados podem ser enviados.");
        Status = OrderStatus.Shipped;
    }

    public void Cancel()
    {
        if (Status is not (OrderStatus.Created or OrderStatus.Confirmed)) throw new DomainException("Pedido não pode ser cancelado neste estado.");
        Status = OrderStatus.Cancelled;
    }
}

public sealed record Product
{
    public Product(Guid id, string name, string sku, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(sku) || price < 0)
            throw new DomainException("Produto inválido.");
        Id = id; Name = name; Sku = sku; Price = price;
    }
    public Guid Id { get; }
    public string Name { get; }
    public string Sku { get; }
    public decimal Price { get; }
}

public sealed record OrderItem(Guid ProductId, string ProductName, string Sku, decimal UnitPrice, int Quantity);
public sealed class DomainException(string message) : Exception(message);
