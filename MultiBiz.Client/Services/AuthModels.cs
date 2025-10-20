namespace MultiBiz.Client.Services;

public record LoginRequest(string UserName, string Password);

public record LoginResponse(
    string token,
    string userName,
    string email,
    string tenantId
);