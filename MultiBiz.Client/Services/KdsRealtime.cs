using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace MultiBiz.Client.Services;

public class KdsRealtime : IAsyncDisposable
{
    private readonly NavigationManager _nav;
    private HubConnection? _conn;
    public event Func<Task>? OnChanged;

    public KdsRealtime(NavigationManager nav) => _nav = nav;

    /// <summary>
    /// Inicia la conexión al hub KDS.
    /// - jwt: si tu hub está con [Authorize], pásalo aquí.
    /// - devTenantSubdomain: en DEV, si quieres forzar el tenant via header (p.ej. "demo").
    /// </summary>
    public async Task StartAsync(string? jwt = null, string? devTenantSubdomain = null)
    {
        if (_conn != null) return;

        var baseUri = _nav.BaseUri.TrimEnd('/');

        _conn = new HubConnectionBuilder()
            .WithUrl($"{baseUri}/hubs/kds", options =>
            {
                if (!string.IsNullOrWhiteSpace(jwt))
                    options.AccessTokenProvider = () => Task.FromResult(jwt)!;

#if DEBUG
                // Solo en dev: permite forzar el tenant por header
                if (!string.IsNullOrWhiteSpace(devTenantSubdomain))
                    options.Headers.Add("X-Tenant-Subdomain", devTenantSubdomain);
#endif
            })
            .WithAutomaticReconnect()
            .Build();

        _conn.On<object>("OrderCreated", async _ => { if (OnChanged != null) await OnChanged.Invoke(); });
        _conn.On<object>("OrderStatusChanged", async _ => { if (OnChanged != null) await OnChanged.Invoke(); });

        await _conn.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_conn != null)
        {
            await _conn.DisposeAsync();
            _conn = null;
        }
    }
}