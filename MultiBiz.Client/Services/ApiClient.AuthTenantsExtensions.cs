using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MultiBiz.Client.Services
{
    public static class ApiClientAuthTenantsExtensions
    {
        private static async Task<TOut?> PostWithFallback<TIn, TOut>(this HttpClient http, TIn body, params string[] paths)
        {
            foreach (var p in paths)
            {
                try
                {
                    var resp = await http.PostAsJsonAsync(p, body);
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        continue;
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadFromJsonAsync<TOut>();
                }
                catch (HttpRequestException)
                {
                    // intentar siguiente path
                }
            }
            throw new InvalidOperationException($"No se encontró un endpoint válido para: {string.Join(", ", paths)}");
        }

        private static async Task<bool> PostNoResultWithFallback<TIn>(this HttpClient http, TIn body, params string[] paths)
        {
            foreach (var p in paths)
            {
                try
                {
                    var resp = await http.PostAsJsonAsync(p, body);
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        continue;
                    resp.EnsureSuccessStatusCode();
                    return true;
                }
                catch (HttpRequestException)
                {
                    // intentar siguiente path
                }
            }
            return false;
        }

        // ---------- AUTH ----------
        // antes:
        // public static Task<JsonElement?> Login(this ApiClient api, string userName, string password)
        // después:
        public static Task<JsonElement> Login(this ApiClient api, string userName, string password)
        {
            var body = new { userName, password };
            return api.Http.PostWithFallback<object, JsonElement>(
                body,
                "/api/auth/login",
                "/api/account/login",
                "/api/users/login"
            )!;
        }

        // antes:
        // public static Task<JsonElement?> Register(this ApiClient api, string userName, string password, string name, string email, Guid tenantId)
        // después:
        public static Task<JsonElement> Register(this ApiClient api, string userName, string password, string name, string email, Guid tenantId)
        {
            var body = new { userName, password, name, email, tenantId };
            return api.Http.PostWithFallback<object, JsonElement>(
                body,
                "/api/auth/register",
                "/api/account/register",
                "/api/users/register"
            )!;
        }

        public static Task<bool> ForgotPassword(this ApiClient api, string email)
        {
            var body = new { email };
            return api.Http.PostNoResultWithFallback<object>(
                body,
                "/api/auth/forgot",
                "/api/account/forgot",
                "/api/users/forgot"
            );
        }

        public static Task<bool> ResetPassword(this ApiClient api, string email, string token, string newPassword)
        {
            var body = new { email, token, newPassword };
            return api.Http.PostNoResultWithFallback<object>(
                body,
                "/api/auth/reset",
                "/api/account/reset",
                "/api/users/reset"
            );
        }
    }
}
