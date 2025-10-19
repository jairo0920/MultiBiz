namespace MultiBiz.Shared.Restaurant;
public enum OrderStatus { New, InProgress, Ready, Served, Paid, Cancelled }
public record TableDto(Guid Id, string Name, int Capacity, Guid TenantId);
public record MenuItemDto(Guid Id, string Name, string Category, decimal Price, Guid TenantId);
public record OrderItemDto(Guid MenuItemId, int Quantity, decimal UnitPrice);
public record OrderDto(Guid Id, Guid TableId, List<OrderItemDto> Items, OrderStatus Status, decimal Subtotal, decimal Tip, decimal Tax, decimal Total, Guid TenantId);
public record CreateOrderRequest(Guid TableId, List<OrderItemDto> Items, decimal TipPercent);
public record KdsOrderItem(Guid OrderId, string Table, OrderStatus Status, IEnumerable<(string Item, int Qty)> Lines);
