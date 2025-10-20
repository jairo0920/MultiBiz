using System.Net.Http;

namespace MultiBiz.Client.Services
{
    public partial class ApiClient
    {
        public HttpClient Http { get; }

        public ApiClient(HttpClient http)
        {
            Http = http;
        }
    }
}
