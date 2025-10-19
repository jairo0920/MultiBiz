namespace MultiBiz.Shared.Roles;
public record RoleDto(Guid Id, string Name, Guid TenantId);
public record CreateRoleRequest(string Name, Guid TenantId);
public record AssignRoleRequest(Guid UserId, Guid RoleId);
