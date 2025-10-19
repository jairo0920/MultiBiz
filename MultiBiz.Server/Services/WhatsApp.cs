using System.Net.Http.Headers;

namespace MultiBiz.Server.Services;
public interface IWhatsAppSender
{
    Task SendTemplateAsync(string toPhoneE164, string templateName, Dictionary<string,string>? vars = null);
}

public class MetaCloudWhatsAppSender : IWhatsAppSender
{
    private readonly HttpClient _http = new();
    private readonly IConfiguration _cfg;
    public MetaCloudWhatsAppSender(IConfiguration cfg) => _cfg = cfg;

    public async Task SendTemplateAsync(string toPhoneE164, string templateName, Dictionary<string,string>? vars = null)
    {
        var token = _cfg["WhatsApp:MetaCloudApiToken"];
        var phoneId = _cfg["WhatsApp:PhoneNumberId"];
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(phoneId)) return;

        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var url = $"https://graph.facebook.com/v19.0/{phoneId}/messages";
        var components = new List<object>();
        if (vars != null && vars.Count > 0)
        {
            var bodyParams = vars.Select(v => new { type = "text", text = v.Value }).ToArray();
            components.Add(new { type = "body", parameters = bodyParams });
        }
        var payload = new
        {
            messaging_product = "whatsapp",
            to = toPhoneE164,
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = "es" },
                components = components
            }
        };
        var res = await _http.PostAsJsonAsync(url, payload);
        res.EnsureSuccessStatusCode();
    }
}
