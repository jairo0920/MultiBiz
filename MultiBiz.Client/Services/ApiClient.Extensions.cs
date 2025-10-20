using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using MultiBiz.Shared.Abstractions;
using MultiBiz.Shared.Users;
using MultiBiz.Shared.Tenants;
using MultiBiz.Shared.Restaurant;
using MultiBiz.Shared.Barbershop;

namespace MultiBiz.Client.Services
{
    public static class ApiClientExtensions
    {
        private static async Task<T?> GetWithFallback<T>(this HttpClient http, params string[] paths)
        {
            foreach (var p in paths)
            {
                try
                {
                    var resp = await http.GetAsync(p);
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                        continue;
                    resp.EnsureSuccessStatusCode();
                    return await resp.Content.ReadFromJsonAsync<T>();
                }
                catch (HttpRequestException)
                {
                    // probar siguiente path
                }
            }
            throw new InvalidOperationException($"No se encontró un endpoint válido para: {string.Join(", ", paths)}");
        }

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
                    // probar siguiente path
                }
            }
            throw new InvalidOperationException($"No se encontró un endpoint válido para: {string.Join(", ", paths)}");
        }

        // ======== USUARIOS ========
        public static async Task<PagedResult<UserDto>> GetUsers(this ApiClient api, int page, int pageSize, string? q)
        {
            var http = api.Http;
            q ??= string.Empty;
            var qs = $"?page={page}&pageSize={pageSize}&q={Uri.EscapeDataString(q)}";
            return await http.GetWithFallback<PagedResult<UserDto>>(
                $"/api/admin/users{qs}",
                $"/api/users{qs}",
                $"/api/administration/users{qs}"
            ) ?? new PagedResult<UserDto>(Array.Empty<UserDto>(), page, pageSize, 0);
        }

        // ======== TENANTS ========
        public static async Task<PagedResult<TenantDto>> GetTenants(this ApiClient api, int page, int pageSize, string? q)
        {
            var http = api.Http;
            q ??= string.Empty;
            var qs = $"?page={page}&pageSize={pageSize}&q={Uri.EscapeDataString(q)}";
            return await http.GetWithFallback<PagedResult<TenantDto>>(
                $"/api/admin/tenants{qs}",
                $"/api/tenants{qs}",
                $"/api/administration/tenants{qs}"
            ) ?? new PagedResult<TenantDto>(Array.Empty<TenantDto>(), page, pageSize, 0);
        }

        public static async Task<TenantDto?> CreateTenant(this ApiClient api, CreateTenantRequest req)
        {
            var http = api.Http;
            return await http.PostWithFallback<CreateTenantRequest, TenantDto>(
                req,
                "/api/admin/tenants",
                "/api/tenants",
                "/api/administration/tenants"
            );
        }

        // ======== AUTENTICACIÓN ========
        public static async Task<JsonElement?> RegisterUser(this ApiClient api, string userName, string password, string name, string email, Guid tenantId)
        {
            var http = api.Http;
            var body = new { userName, password, name, email, tenantId };
            return await http.PostWithFallback<object, JsonElement>(
                body,
                "/api/auth/register",
                "/api/account/register",
                "/api/users/register"
            );
        }

        public static async Task<JsonElement?> LoginRaw(this ApiClient api, string userName, string password)
        {
            var http = api.Http;
            var body = new { userName, password };
            return await http.PostWithFallback<object, JsonElement>(
                body,
                "/api/auth/login",
                "/api/account/login",
                "/api/users/login"
            );
        }

        // ======== RESTAURANTE: KDS ========
        public static async Task<IEnumerable<KdsOrderItem>> GetKds(this ApiClient api, int page = 1, int pageSize = 20, string? q = null)
        {
            var http = api.Http;
            q ??= string.Empty;
            var qs = $"?page={page}&pageSize={pageSize}&q={Uri.EscapeDataString(q)}";
            return await http.GetWithFallback<IEnumerable<KdsOrderItem>>(
                $"/api/restaurant/kds{qs}",
                $"/api/restaurant/orders/kds{qs}",
                $"/api/kds{qs}"
            ) ?? Enumerable.Empty<KdsOrderItem>();
        }

        // ======== BARBERÍA: Citas ========
        public static async Task<IEnumerable<AppointmentDto>> GetAppointments(this ApiClient api, DateTime? from = null, DateTime? to = null)
        {
            var http = api.Http;
            var f = (from ?? DateTime.UtcNow.Date).ToString("O");
            var t = (to ?? DateTime.UtcNow.Date.AddDays(7)).ToString("O");
            var qs = $"?from={Uri.EscapeDataString(f)}&to={Uri.EscapeDataString(t)}";
            return await http.GetWithFallback<IEnumerable<AppointmentDto>>(
                $"/api/barbershop/appointments{qs}",
                $"/api/appointments{qs}",
                $"/api/barbershop/calendar{qs}"
            ) ?? Enumerable.Empty<AppointmentDto>();
        }
    }
}
