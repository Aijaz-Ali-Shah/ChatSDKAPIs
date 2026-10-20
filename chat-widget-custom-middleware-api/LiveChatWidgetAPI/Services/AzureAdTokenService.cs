using LiveChatWidgetAPI.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiveChatWidgetAPI.Services
{
    public class AzureAdTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AzureAdTokenService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<TokenResponse?> GetAccessTokenAsync()
        {
            var tenantId = _config["AzureAd:TenantId"];
            var clientId = _config["AzureAd:ClientId"];
            var clientSecret = _config["AzureAd:ClientSecret"];
            var scope = _config["AzureAd:Scope"];

            var url = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var requestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId! },
            { "client_secret", clientSecret! },
            { "scope", scope! }
        };

            var content = new FormUrlEncodedContent(requestBody);

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Token request failed: {response.StatusCode} - {responseString}");
            }

            return JsonSerializer.Deserialize<TokenResponse>(responseString);
        }
    }

   
}
