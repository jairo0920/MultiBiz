using System.Net.Http.Json;
using MultiBiz.Shared.Tenants;

namespace MultiBiz.Client.Services
{
    /// <summary>
    /// Extensiones extra para cubrir Login/Register y CreateTenant(CreateTenantRequest).
    /// </summary>
    public partial class ApiClient
    {
        // ========== AUTH ==========
        public async Task<object?> Login(string userName, string password)
        {
            var res = await _http.PostAsJsonAsync("api/auth/login", new { UserName = userName, Password = password });
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<object>();
        }

        // Acepta cualquier DTO de registro (lo serializa tal cual)
        public async Task<object?> Register(object request)
        {
            var res = await _http.PostAsJsonAsync("api/auth/register", request);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<object>();
        }

        // ========== TENANTS ==========
        public async Task<TenantDto?> CreateTenant(CreateTenantRequest req)
        {
            var res = await _http.PostAsJsonAsync("api/admin/tenants", req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TenantDto>();
        }

        // Fallback genérico por si tu UI usa otro shape
        public async Task<object?> CreateTenant(object req)
        {
            var res = await _http.PostAsJsonAsync("api/admin/tenants", req);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<object>();
        }
    }
}
