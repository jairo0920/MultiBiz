using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Data;
using MultiBiz.Server.Models;
using MultiBiz.Server.Tenants;
using MultiBiz.Shared.Barbershop;

namespace MultiBiz.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BarbershopController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    public BarbershopController(AppDbContext db, ITenantProvider tenant) { _db = db; _tenant = tenant; }

    [HttpGet("calendar")]
    public async Task<IEnumerable<AppointmentDto>> Calendar(DateTime? from = null, DateTime? to = null)
    {
        var start = from ?? DateTime.UtcNow.Date;
        var end = to ?? start.AddDays(7);
        var q = _db.Appointments.AsNoTracking().Where(a => a.Start >= start && a.Start < end);
        var items = await q.ToListAsync();
        return items.Select(a => new AppointmentDto(a.Id, a.BarberId, a.ServiceId, a.Start, a.CustomerName, a.CustomerPhone, a.Confirmed, a.TenantId));
    }

    [HttpPost("appointment")]
    public async Task<ActionResult> CreateAppointment(CreateAppointmentRequest req)
    {
        var ap = new Appointment { TenantId = _tenant.TenantId, BarberId = req.BarberId, ServiceId = req.ServiceId, Start = req.Start, CustomerName = req.CustomerName, CustomerPhone = req.CustomerPhone };
        _db.Appointments.Add(ap);
        await _db.SaveChangesAsync();
        return Ok(new { ap.Id });
    }
}
