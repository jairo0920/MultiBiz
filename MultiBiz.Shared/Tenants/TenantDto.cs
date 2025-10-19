namespace MultiBiz.Shared.Tenants;
public record TenantDto(Guid Id, string Name, string Subdomain, bool IsActive);
public record CreateTenantRequest(string Name, string Subdomain, bool IsActive);
public record UpdateTenantRequest(string Name, string Subdomain, bool IsActive);
