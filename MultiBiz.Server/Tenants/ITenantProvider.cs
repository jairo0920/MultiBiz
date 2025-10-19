namespace MultiBiz.Server.Tenants;
public interface ITenantProvider
{
    Guid TenantId { get; }
    string? Subdomain { get; }
    void SetTenant(Guid id, string? subdomain);
}
