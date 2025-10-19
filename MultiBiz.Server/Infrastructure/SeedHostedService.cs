using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Data;
using MultiBiz.Server.Models;
using MultiBiz.Server.Tenants;
using MultiBiz.Shared.Modules;

namespace MultiBiz.Server.Infrastructure;

public class SeedOptions
{
    public bool Enable { get; set; } = true;
    public string AdminUserName { get; set; } = "admin";
    public string AdminEmail { get; set; } = "admin@demo.local";
    public string AdminFullName { get; set; } = "Administrador Demo";
    public string AdminPassword { get; set; } = "Admin123!";
    public string TenantName { get; set; } = "Demo Company";
    public string TenantSubdomain { get; set; } = "demo";
}

public class SeedHostedService : IHostedService
{
    private readonly IServiceProvider _sp;
    private readonly IConfiguration _cfg;
    public SeedHostedService(IServiceProvider sp, IConfiguration cfg){ _sp = sp; _cfg = cfg; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var opts = _cfg.GetSection("Seed").Get<SeedOptions>() ?? new SeedOptions();
        if (!opts.Enable) return;

        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var rm = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.MigrateAsync(cancellationToken);

        var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Subdomain == opts.TenantSubdomain, cancellationToken);
        if (tenant == null)
        {
            tenant = new Tenant { Name = opts.TenantName, Subdomain = opts.TenantSubdomain, IsActive = true };
            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(cancellationToken);
        }

        tenantProvider.SetTenant(tenant.Id, tenant.Subdomain);

        foreach (var r in new[] { "Admin", "Manager", "Staff" })
        {
            if (!await rm.RoleExistsAsync(r))
                await rm.CreateAsync(new ApplicationRole { Name = r });
        }

        var admin = await um.FindByNameAsync(opts.AdminUserName);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = opts.AdminUserName,
                Email = opts.AdminEmail,
                FullName = opts.AdminFullName,
                TenantId = tenant.Id,
                IsActive = true,
                EmailConfirmed = true
            };
            var res = await um.CreateAsync(admin, opts.AdminPassword);
            if (!res.Succeeded)
            {
                var msg = string.Join("; ", res.Errors.Select(e => $"{e.Code}:{e.Description}"));
                throw new Exception("Seed admin failed: " + msg);
            }
            await um.AddToRoleAsync(admin, "Admin");
        }

        if (!await db.Modules.AnyAsync(cancellationToken))
        {
            db.Modules.Add(new SystemModule { TenantId = tenant.Id, Name = "Restaurante", Type = ModuleType.Restaurant, IsEnabled = true });
            db.Modules.Add(new SystemModule { TenantId = tenant.Id, Name = "Barbería", Type = ModuleType.Barbershop, IsEnabled = true });
        }

        if (!await db.Tables.AnyAsync(cancellationToken))
        {
            db.Tables.AddRange(
                new Models.Table { TenantId = tenant.Id, Name = "Mesa 1", Capacity = 2 },
                new Models.Table { TenantId = tenant.Id, Name = "Mesa 2", Capacity = 4 },
                new Models.Table { TenantId = tenant.Id, Name = "Barra", Capacity = 6 }
            );
        }
        if (!await db.MenuItems.AnyAsync(cancellationToken))
        {
            db.MenuItems.AddRange(
                new Models.MenuItem { TenantId = tenant.Id, Name = "Café", Category = "Bebidas", Price = 1000 },
                new Models.MenuItem { TenantId = tenant.Id, Name = "Sandwich", Category = "Comida", Price = 2500 }
            );
        }
        await db.SaveChangesAsync(cancellationToken);

        if (!await db.Barbers.AnyAsync(cancellationToken))
        {
            var b1 = new Models.Barber { TenantId = tenant.Id, Name = "Carlos" };
            var b2 = new Models.Barber { TenantId = tenant.Id, Name = "María" };
            db.Barbers.AddRange(b1, b2);
            await db.SaveChangesAsync(cancellationToken);

            var s1 = new Models.Service { TenantId = tenant.Id, Name = "Corte básico", Minutes = 30, Price = 4000 };
            var s2 = new Models.Service { TenantId = tenant.Id, Name = "Corte + Barba", Minutes = 45, Price = 6000 };
            db.Services.AddRange(s1, s2);
            await db.SaveChangesAsync(cancellationToken);

            db.Appointments.Add(new Models.Appointment
            {
                TenantId = tenant.Id,
                BarberId = b1.Id,
                ServiceId = s1.Id,
                Start = DateTime.UtcNow.AddHours(3),
                CustomerName = "Cliente Demo",
                CustomerPhone = "+50670000000",
                Confirmed = true
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
