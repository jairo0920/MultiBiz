using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MultiBiz.Client;
using MultiBiz.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Servicios de Auth + Storage
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();

// DelegatingHandler que adjunta Authorization: Bearer <token>
builder.Services.AddTransient<AuthHttpMessageHandler>();

// URL base de la API según entorno
var apiBaseUrl = builder.HostEnvironment.IsDevelopment()
    ? "https://demo.localtest.me:5001" // Desarrollo
    : "https://api.tu-dominio.com";    // Producción (ajustar dominio)

// HttpClient hacia la API con handler y InnerHandler asignado
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthHttpMessageHandler>();
    handler.InnerHandler = new HttpClientHandler(); // <- necesario para evitar “The inner handler has not been assigned”
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
});

// Tu cliente de API
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();