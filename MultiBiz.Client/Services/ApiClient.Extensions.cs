using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using MultiBiz.Shared.Abstractions;
using MultiBiz.Shared.Users;
using MultiBiz.Shared.Tenants;
using MultiBiz.Shared.Barbershop;
using MultiBiz.Shared.Restaurant;

namespace MultiBiz.Client.Services
{
    /// <summary>
    /// Extensiones tipadas del ApiClient para que coincidan con las páginas Blazor.
    /// Deja este archivo junto a tu ApiClient.cs (ambos deben ser 'partial').
    /// </summary>
    public partial class ApiClient
    {
        // ==================== RESTAURANT / KDS ====================
        public Task<IEnumerable<KdsOrderItem>?> GetKds()
            => _http.GetFromJsonAsync<IEnumerable<KdsOrderItem>>("api/restaurant/kds");

        // ==================== ADMIN: USERS ====================
        public Task<PagedResult<UserDto>?> GetUsers(int page = 1, int pageSize = 50, string? q = null)
        {
            var query = Uri.EscapeDataString(q ?? string.Empty);
            return _http.GetFromJsonAsync<PagedResult<UserDto>>($"api/admin/users?page={page}&pageSize={pageSize}&q={query}");
        }

        // ==================== ADMIN: TENANTS ====================
        public Task<PagedResult<TenantDto>?> GetTenants(int page = 1, int pageSize = 50, string? q = null)
        {
            var query = Uri.EscapeDataString(q ?? string.Empty);
            return _http.GetFromJsonAsync<PagedResult<TenantDto>>($"api/admin/tenants?page={page}&pageSize={pageSize}&q={query}");
        }

        // Overload conveniente si tu UI solo captura el nombre: genera subdominio slug.
        public Task<TenantDto?> CreateTenant(string name) => CreateTenant(name, Slugify(name), isActive: true);

        public async Task<TenantDto?> CreateTenant(string name, string subdomain, bool isActive = true)
        {
            var res = await _http.PostAsJsonAsync("api/admin/tenants", new { Name = name, Subdomain = subdomain, IsActive = isActive });
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<TenantDto>();
        }

        // ==================== BARBERSHOP: CALENDAR ====================
        public Task<IEnumerable<AppointmentDto>?> GetAppointments(string? from = null, string? to = null)
        {
            var q = new List<string>();
            if (!string.IsNullOrWhiteSpace(from)) q.Add($"from={Uri.EscapeDataString(from)}");
            if (!string.IsNullOrWhiteSpace(to)) q.Add($"to={Uri.EscapeDataString(to)}");
            var url = "api/barbershop/appointments" + (q.Count > 0 ? "?" + string.Join("&", q) : "");
            return _http.GetFromJsonAsync<IEnumerable<AppointmentDto>>(url);
        }

        // ==================== Helpers ====================
        private static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            // Quitar acentos
            var normalized = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            var noAccents = sb.ToString().Normalize(NormalizationForm.FormC);
            // Reemplazos básicos
            var slug = new string(noAccents.Select(ch =>
            {
                if (char.IsLetterOrDigit(ch)) return ch;
                if (char.IsWhiteSpace(ch) || ch == '_' || ch == '-' ) return '-';
                return '\0';
            }).Where(ch => ch != '\0').ToArray());

            // Colapsar múltiples '-' y recortar
            while (slug.Contains("--")) slug = slug.Replace("--", "-");
            slug = slug.Trim('-');
            return slug;
        }
    }
}
