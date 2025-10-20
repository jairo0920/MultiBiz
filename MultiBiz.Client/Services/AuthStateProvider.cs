using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

namespace MultiBiz.Client.Services;

public class AuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public AuthStateProvider(ILocalStorageService storage)
    {
        _storage = storage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsync<string>("authToken");
        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(Anonymous);

        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        return new AuthenticationState(user);
    }

    public void MarkUserAsAuthenticated(string token)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public void MarkUserAsLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var parts = jwt.Split('.');
        if (parts.Length < 2) return claims;

        string payload = parts[1];
        // Base64Url -> Base64
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        var jsonBytes = Convert.FromBase64String(payload);
        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes) ?? new();

        foreach (var kv in dict)
        {
            if (kv.Value is JsonElement je)
            {
                if (je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in je.EnumerateArray())
                        claims.Add(new Claim(kv.Key, item.ToString()));
                }
                else
                {
                    claims.Add(new Claim(kv.Key, je.ToString()));
                }
            }
            else
            {
                claims.Add(new Claim(kv.Key, kv.Value?.ToString() ?? ""));
            }
        }
        return claims;
    }
}
