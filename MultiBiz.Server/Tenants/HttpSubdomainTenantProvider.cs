using System.Text.RegularExpressions;

namespace MultiBiz.Server.Tenants;
public class HttpSubdomainTenantProvider : ITenantProvider
{
    private static readonly AsyncLocal<Guid> _tenant = new();
    private static readonly AsyncLocal<string?> _sub = new();

    public Guid TenantId => _tenant.Value;
    public string? Subdomain => _sub.Value;

    public void SetTenant(Guid id, string? subdomain)
    {
        _tenant.Value = id;
        _sub.Value = subdomain;
    }
}
