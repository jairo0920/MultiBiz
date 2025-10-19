using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Data;
using MultiBiz.Server.Hubs;
using MultiBiz.Server.Models;
using MultiBiz.Server.Tenants;
using MultiBiz.Shared.Abstractions;
using MultiBiz.Shared.Restaurant;

namespace MultiBiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IHubContext<KdsHub> _hub;

    public RestaurantController(AppDbContext db, ITenantProvider tenant, IHubContext<KdsHub> hub)
    {
        _db = db; _tenant = tenant; _hub = hub;
    }

    public record CreateTableRequest(string Name, int Capacity);
    public record UpdateTableRequest(string Name, int Capacity);
    public record CreateMenuItemRequest(string Name, string Category, decimal Price);
    public record UpdateMenuItemRequest(string Name, string Category, decimal Price);
    public record UpdateOrderStatusRequest(OrderStatus Status);

    [HttpGet("tables")]
    public async Task<PagedResult<TableDto>> GetTables(int page = 1, int pageSize = 50, string? q = null)
    {
        var query = _db.Tables.AsNoTracking().Where(t => t.TenantId == _tenant.TenantId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(t => t.Name.Contains(q));
        var total = await query.CountAsync();
        var items = await query.OrderBy(t => t.Name).Skip((page-1)*pageSize).Take(pageSize)
            .Select(t => new TableDto(t.Id, t.Name, t.Capacity, t.TenantId)).ToListAsync();
        return new PagedResult<TableDto>(items, total, page, pageSize);
    }

    [HttpGet("tables/{id:guid}")]
    public async Task<ActionResult<TableDto>> GetTable(Guid id)
    {
        var t = await _db.Tables.AsNoTracking().FirstOrDefaultAsync(x => x.Id==id && x.TenantId==_tenant.TenantId);
        if (t == null) return NotFound();
        return new TableDto(t.Id, t.Name, t.Capacity, t.TenantId);
    }

    [HttpPost("tables")]
    public async Task<ActionResult<TableDto>> CreateTable([FromBody] CreateTableRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Nombre requerido.");
        if (req.Capacity <= 0) return BadRequest("Capacity debe ser > 0.");
        var entity = new Table { TenantId = _tenant.TenantId, Name=req.Name.Trim(), Capacity=req.Capacity };
        _db.Tables.Add(entity);
        await _db.SaveChangesAsync();
        var dto = new TableDto(entity.Id, entity.Name, entity.Capacity, entity.TenantId);
        return CreatedAtAction(nameof(GetTable), new { id = entity.Id }, dto);
    }

    [HttpPut("tables/{id:guid}")]
    public async Task<ActionResult> UpdateTable(Guid id, [FromBody] UpdateTableRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Nombre requerido.");
        if (req.Capacity <= 0) return BadRequest("Capacity debe ser > 0.");
        var entity = await _db.Tables.FirstOrDefaultAsync(x => x.Id==id && x.TenantId==_tenant.TenantId);
        if (entity == null) return NotFound();
        entity.Name = req.Name.Trim(); entity.Capacity = req.Capacity;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("tables/{id:guid}")]
    public async Task<ActionResult> DeleteTable(Guid id)
    {
        var entity = await _db.Tables.FirstOrDefaultAsync(x => x.Id==id && x.TenantId==_tenant.TenantId);
        if (entity == null) return NotFound();
        _db.Tables.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("menu")]
    public async Task<PagedResult<MenuItemDto>> GetMenu(int page=1, int pageSize=50, string? q=null, string? category=null)
    {
        var query = _db.MenuItems.AsNoTracking().Where(m => m.TenantId == _tenant.TenantId);
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(m => m.Name.Contains(q) || m.Category.Contains(q));
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(m => m.Category == category);
        var total = await query.CountAsync();
        var items = await query.OrderBy(m=>m.Category).ThenBy(m=>m.Name).Skip((page-1)*pageSize).Take(pageSize)
            .Select(m => new MenuItemDto(m.Id, m.Name, m.Category, m.Price, m.TenantId)).ToListAsync();
        return new PagedResult<MenuItemDto>(items, total, page, pageSize);
    }

    [HttpGet("menu/{id:guid}")]
    public async Task<ActionResult<MenuItemDto>> GetMenuItem(Guid id)
    {
        var m = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id==id && x.TenantId==_tenant.TenantId);
        if (m == null) return NotFound();
        return new MenuItemDto(m.Id, m.Name, m.Category, m.Price, m.TenantId);
    }

    [HttpPost("menu")]
    public async Task<ActionResult<MenuItemDto>> CreateMenuItem([FromBody] CreateMenuItemRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Nombre requerido.");
        if (string.IsNullOrWhiteSpace(req.Category)) return BadRequest("Categoría requerida.");
        if (req.Price <= 0) return BadRequest("Precio debe ser > 0.");
        var entity = new MenuItem { TenantId=_tenant.TenantId, Name=req.Name.Trim(), Category=req.Category.Trim(), Price=Math.Round(req.Price,2) };
        _db.MenuItems.Add(entity);
        await _db.SaveChangesAsync();
        var dto = new MenuItemDto(entity.Id, entity.Name, entity.Category, entity.Price, entity.TenantId);
        return CreatedAtAction(nameof(GetMenuItem), new { id = entity.Id }, dto);
    }

    [HttpPut("menu/{id:guid}")]
    public async Task<ActionResult> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) return BadRequest("Nombre requerido.");
        if (string.IsNullOrWhiteSpace(req.Category)) return BadRequest("Categoría requerida.");
        if (req.Price <= 0) return BadRequest("Precio debe ser > 0.");
        var entity = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id==id && x.TenantId==_tenant.TenantId);
        if (entity == null) return NotFound();
        entity.Name=req.Name.Trim(); entity.Category=req.Category.Trim(); entity.Price=Math.Round(req.Price,2);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("menu/{id:guid}")]
    public async Task<ActionResult> DeleteMenuItem(Guid id)
    {
        var entity = await _db.MenuItems.FirstOrDefaultAsync(x => x.Id==id && x.TenantId==_tenant.TenantId);
        if (entity == null) return NotFound();
        _db.MenuItems.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("order")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest req)
    {
        if (req == null) return BadRequest("Body requerido.");
        if (req.TableId == Guid.Empty) return BadRequest("TableId inválido.");
        if (req.Items == null || req.Items.Count == 0) return BadRequest("Debe enviar al menos un item.");
        if (req.Items.Any(i => i.MenuItemId == Guid.Empty || i.Quantity <= 0 || i.UnitPrice <= 0))
            return BadRequest("Items con datos inválidos.");

        var existsTable = await _db.Tables.AnyAsync(t => t.Id == req.TableId && t.TenantId == _tenant.TenantId);
        if (!existsTable) return BadRequest("La mesa no existe.");

        var menuItemIds = req.Items.Select(i => i.MenuItemId).Distinct().ToList();
        var existsAll = await _db.MenuItems.Where(m => m.TenantId == _tenant.TenantId && menuItemIds.Contains(m.Id)).CountAsync() == menuItemIds.Count;
        if (!existsAll) return BadRequest("Uno o más MenuItemId no existen.");

        var order = new Order { TenantId=_tenant.TenantId, TableId=req.TableId, Status=OrderStatus.New };
        foreach (var it in req.Items)
        {
            order.Lines.Add(new OrderLine{ MenuItemId=it.MenuItemId, Quantity=it.Quantity, UnitPrice=it.UnitPrice });
            order.Subtotal += it.UnitPrice * it.Quantity;
        }
        order.Tip = Math.Round(order.Subtotal * req.TipPercent / 100m, 2);
        order.Tax = Math.Round(order.Subtotal * 0.13m, 2);
        order.Total = order.Subtotal + order.Tip + order.Tax;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        await _hub.Clients.Group(_tenant.TenantId.ToString()).SendAsync("OrderCreated", new {
            order.Id, order.TableId, order.Status, order.Subtotal, order.Tip, order.Tax, order.Total
        });

        var dto = new OrderDto(order.Id, order.TableId,
            order.Lines.Select(l => new OrderItemDto(l.MenuItemId, l.Quantity, l.UnitPrice)).ToList(),
            order.Status, order.Subtotal, order.Tip, order.Tax, order.Total, order.TenantId);

        return Ok(dto);
    }

    [HttpPatch("order/{id:guid}/status")]
    public async Task<ActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id==id && o.TenantId==_tenant.TenantId);
        if (order == null) return NotFound();
        order.Status = req.Status;
        await _db.SaveChangesAsync();
        await _hub.Clients.Group(_tenant.TenantId.ToString()).SendAsync("OrderStatusChanged", new { order.Id, order.Status });
        return NoContent();
    }

    [HttpGet("kds")]
    public async Task<IEnumerable<KdsOrderItem>> Kds()
    {
        var orders = await _db.Orders.Include(o=>o.Table).Include(o=>o.Lines)
            .Where(o=>o.TenantId==_tenant.TenantId).OrderBy(o=>o.Status).ToListAsync();
        return orders.Select(o => new KdsOrderItem(
            o.Id, o.Table?.Name ?? "N/A", o.Status,
            o.Lines.Select(l => (Item: l.MenuItemId.ToString(), Qty: l.Quantity))
        ));
    }
}
