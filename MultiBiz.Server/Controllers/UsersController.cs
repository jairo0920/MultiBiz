using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Data;
using MultiBiz.Server.Models;
using MultiBiz.Server.Tenants;
using MultiBiz.Shared.Abstractions;
using MultiBiz.Shared.Users;

namespace MultiBiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _um;
    private readonly ITenantProvider _tenant;

    public UsersController(AppDbContext db, UserManager<ApplicationUser> um, ITenantProvider tenant)
    {
        _db = db; _um = um; _tenant = tenant;
    }

    [HttpGet]
    public async Task<PagedResult<UserDto>> Get(int page = 1, int pageSize = 20)
    {
        var q = _db.Users.AsNoTracking().Where(u => u.TenantId == _tenant.TenantId);
        var total = await q.CountAsync();
        var items = await q.OrderBy(u => u.UserName).Skip((page-1)*pageSize).Take(pageSize).Select(u =>
            new UserDto(u.Id, u.UserName!, u.Email!, u.FullName, u.IsActive, u.TenantId)).ToListAsync();
        return new PagedResult<UserDto>(items, total, page, pageSize);
    }

    [HttpPost]
    public async Task<ActionResult> Create(CreateUserRequest req)
    {
        var u = new ApplicationUser
        {
            UserName = req.UserName, Email = req.Email, FullName = req.FullName, TenantId = req.TenantId, IsActive = true
        };
        var res = await _um.CreateAsync(u, req.Password);
        if (!res.Succeeded) return BadRequest(res.Errors);
        return Ok(new UserDto(u.Id, u.UserName!, u.Email!, u.FullName, u.IsActive, u.TenantId));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateUserRequest req)
    {
        var u = await _um.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _tenant.TenantId);
        if (u == null) return NotFound();
        u.Email = req.Email; u.FullName = req.FullName; u.IsActive = req.IsActive;
        await _um.UpdateAsync(u);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var u = await _um.Users.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == _tenant.TenantId);
        if (u == null) return NotFound();
        await _um.DeleteAsync(u);
        return Ok();
    }
}
