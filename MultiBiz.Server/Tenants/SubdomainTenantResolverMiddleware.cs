using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Data;

namespace MultiBiz.Server.Tenants;

public class SubdomainTenantResolverMiddleware
{
    private readonly RequestDelegate _next;
    public SubdomainTenantResolverMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantProvider provider, AppDbContext db)
    {
        // 1) Header opcional para forzar tenant en dev / pruebas (tiene prioridad)
        string? sub = context.Request.Headers["X-Tenant-Subdomain"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(sub))
        {
            // 2) Subdominio del host (e.g. demo.localtest.me)
            var host = context.Request.Host.Host;           // demo.localtest.me, localhost, 127.0.0.1, etc.
            var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries);

            // Heurística simple: si es FQDN (>=3 partes), tomar la primera como subdominio
            // demo.localtest.me -> demo | barberia01.midominio.com -> barberia01
            if (parts.Length >= 3) sub = parts[0];

            // En localhost/127.0.0.1 normalmente no hay subdominio
        }

        // 3) Buscar el tenant sólo si tenemos "sub"
        if (!string.IsNullOrWhiteSpace(sub))
        {
            sub = sub.Trim().ToLowerInvariant();
            var tenant = await db.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == sub && t.IsActive);

            if (tenant != null)
                provider.SetTenant(tenant.Id, sub);
        }

#if DEBUG
        // 4) Fallback DEV: si aún no hay tenant, usar 'demo' automáticamente
        if (provider.TenantId == Guid.Empty)
        {
            var demo = await db.Tenants.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Subdomain == "demo" && t.IsActive);
            if (demo != null)
                provider.SetTenant(demo.Id, demo.Subdomain);
        }
#endif

        await _next(context);
    }
}