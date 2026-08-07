using FluentAssertions;
using OrderFlow.Domain;

namespace OrderFlow.Domain.Tests;

public sealed class OrderTests
{
    private static Product Product() => new(Guid.NewGuid(), "Notebook", "NB-1", 100m);

    [Fact] public void Confirms_a_valid_order() { var order = new Order(); order.AddItem(Product(), 2); order.SetFreight(10); order.Confirm(); order.Status.Should().Be(OrderStatus.Confirmed); }
    [Fact] public void Rejects_order_without_items() { var act = () => new Order().Confirm(); act.Should().Throw<DomainException>(); }
    [Fact] public void Rejects_confirming_cancelled_order() { var order = new Order(); order.AddItem(Product(), 1); order.Cancel(); var act = () => order.Confirm(); act.Should().Throw<DomainException>(); }
    [Fact] public void Calculates_total_and_snapshots_price() { var product = Product(); var order = new Order(); order.AddItem(product, 2); order.SetFreight(15); order.Total.Should().Be(215); order.Items.Single().UnitPrice.Should().Be(100); }
}
