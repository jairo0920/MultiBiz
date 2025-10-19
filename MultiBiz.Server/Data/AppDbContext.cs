using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiBiz.Server.Models;
using MultiBiz.Server.Tenants;
using MultiBiz.Shared.Abstractions;

namespace MultiBiz.Server.Data;
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ITenantProvider _tenantProvider;
    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SystemModule> Modules => Set<SystemModule>();
    public DbSet<UserRoleLink> UserRolesTenant => Set<UserRoleLink>();
    // Restaurant
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    // Barbershop
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Multi-tenant global filters
        foreach (var entityType in b.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext).GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var generic = method!.MakeGenericMethod(entityType.ClrType);
                generic.Invoke(this, new object[] { b });
            }
        }
    }

    private void ApplyTenantFilter<T>(ModelBuilder b) where T : class, ITenantEntity
    {
        b.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
    }
}
