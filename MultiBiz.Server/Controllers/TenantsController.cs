using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Data;
using MultiBiz.Server.Models;
using MultiBiz.Shared.Abstractions;
using MultiBiz.Shared.Tenants;

namespace MultiBiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TenantsController : ControllerBase
{
    private readonly AppDbContext _db;
    public TenantsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<PagedResult<TenantDto>> Get(int page = 1, int pageSize = 20)
    {
        var q = _db.Tenants.AsNoTracking();
        var total = await q.CountAsync();
        var items = await q.OrderBy(t => t.Name).Skip((page-1)*pageSize).Take(pageSize)
            .Select(t => new TenantDto(t.Id, t.Name, t.Subdomain, t.IsActive)).ToListAsync();
        return new PagedResult<TenantDto>(items, total, page, pageSize);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateTenantRequest req)
    {
        if (await _db.Tenants.AnyAsync(t => t.Subdomain == req.Subdomain)) return Conflict("Subdominio en uso");
        var t = new Tenant { Name = req.Name, Subdomain = req.Subdomain, IsActive = req.IsActive };
        _db.Tenants.Add(t);
        await _db.SaveChangesAsync();
        return Ok(new TenantDto(t.Id, t.Name, t.Subdomain, t.IsActive));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateTenantRequest req)
    {
        var t = await _db.Tenants.FindAsync(id);
        if (t == null) return NotFound();
        t.Name = req.Name; t.Subdomain = req.Subdomain; t.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var t = await _db.Tenants.FindAsync(id);
        if (t == null) return NotFound();
        _db.Tenants.Remove(t);
        await _db.SaveChangesAsync();
        return Ok();
    }
}
