using System.Net.Http.Headers;
using Blazored.LocalStorage;

namespace MultiBiz.Client.Services;

public class AuthHttpMessageHandler : DelegatingHandler
{
    private readonly ILocalStorageService _storage;

    public AuthHttpMessageHandler(ILocalStorageService storage)
    {
        _storage = storage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _storage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
