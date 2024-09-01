using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ecclesia.Utility
{
    public class PayPalClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public PayPalClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var clientId = _configuration["PayPal:ClientId"];
            var clientSecret = _configuration["PayPal:ClientSecret"];
            var baseUrl = _configuration["PayPal:BaseUrl"];

            var authToken = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            var content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await _httpClient.PostAsync($"{baseUrl}/v1/oauth2/token", content);

            if (!response.IsSuccessStatusCode)
            {
                // Handle error (e.g., log the error, throw an exception, etc.)
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve PayPal access token. Status Code: {response.StatusCode}. Error: {errorContent}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PayPalAccessTokenResponse>(json);

            return result.access_token;
        }

        public async Task<HttpResponseMessage> MakeAuthorizedRequestAsync(HttpMethod method, string endpoint, HttpContent content = null)
        {
            var accessToken = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(method, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (content != null)
            {
                request.Content = content;
            }

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Request to PayPal API failed. Status Code: {response.StatusCode}. Error: {errorContent}");
            }

            return response;
        }
    }

    public class PayPalAccessTokenResponse
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string app_id { get; set; }
        public int expires_in { get; set; }
        public string nonce { get; set; }
    }
}
