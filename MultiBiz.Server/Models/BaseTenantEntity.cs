using MultiBiz.Shared.Abstractions;

namespace MultiBiz.Server.Models;
public abstract class BaseTenantEntity : ITenantEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
}
