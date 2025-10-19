using MultiBiz.Shared.Restaurant;

namespace MultiBiz.Server.Models;
public class Table : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}

public class MenuItem : BaseTenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

public class Order : BaseTenantEntity
{
    public Guid TableId { get; set; }
    public Table? Table { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.New;
    public decimal Subtotal { get; set; }
    public decimal Tip { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<OrderLine> Lines { get; set; } = new();
}

public class OrderLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid MenuItemId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
