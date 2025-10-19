using Microsoft.AspNetCore.Identity;

namespace MultiBiz.Server.Models;
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid TenantId { get; set; }
}

public class ApplicationRole : IdentityRole<Guid> { }
