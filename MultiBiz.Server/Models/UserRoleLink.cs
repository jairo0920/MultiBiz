namespace MultiBiz.Server.Models;
public class UserRoleLink : BaseTenantEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
