
using System.Net.Http.Json;

namespace MultiBiz.Client.Services
{
    /// <summary>
    /// Overload para Register con 4 parámetros (userName, password, fullName, email),
    /// compatible con Pages/Register.razor.
    /// </summary>
    public partial class ApiClient
    {
        public async Task<object?> Register(string userName, string password, string fullName, string email)
        {
            var body = new { UserName = userName, Password = password, FullName = fullName, Email = email };
            var res = await _http.PostAsJsonAsync("api/auth/register", body);
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<object>();
        }
    }
}
