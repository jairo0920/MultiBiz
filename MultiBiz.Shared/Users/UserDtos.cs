namespace MultiBiz.Shared.Users;
public record UserDto(Guid Id, string UserName, string Email, string FullName, bool IsActive, Guid TenantId);
public record CreateUserRequest(string UserName, string Email, string FullName, string Password, Guid TenantId);
public record UpdateUserRequest(string Email, string FullName, bool IsActive);
public record RegisterRequest(string UserName, string Password, string FullName, string Email);
public record LoginRequest(string UserName, string Password);
public record AuthResponse(string Token, string UserName, string Email, Guid TenantId);
