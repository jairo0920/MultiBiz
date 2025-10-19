using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MultiBiz.Server.Tenants;

namespace MultiBiz.Server.Hubs;

[Authorize] // Opcional: exige JWT si tu app ya emite tokens
public class KdsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var http = Context.GetHttpContext();
        if (http != null)
        {
            var tp = http.RequestServices.GetService(typeof(ITenantProvider)) as ITenantProvider;
            if (tp != null && tp.TenantId != Guid.Empty)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, tp.TenantId.ToString());
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var http = Context.GetHttpContext();
        if (http != null)
        {
            var tp = http.RequestServices.GetService(typeof(ITenantProvider)) as ITenantProvider;
            if (tp != null && tp.TenantId != Guid.Empty)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, tp.TenantId.ToString());
            }
        }
        await base.OnDisconnectedAsync(exception);
    }
}
