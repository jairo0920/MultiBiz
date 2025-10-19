
using System.Net.Http.Json;

namespace MultiBiz.Client.Services;

public partial class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // === Password Reset ===
    public Task ForgotPassword(string userOrEmail)
        => _http.PostAsJsonAsync("api/password/forgot", new { UserNameOrEmail = userOrEmail });

    public async Task ResetPassword(string user, string token, string newPass)
    {
        var res = await _http.PostAsJsonAsync("api/password/reset", new { UserName = user, Token = token, NewPassword = newPass });
        res.EnsureSuccessStatusCode();
    }
}
