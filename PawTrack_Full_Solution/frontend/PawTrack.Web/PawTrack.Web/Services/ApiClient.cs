using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PawTrack.Web.Services
{
    // Thin wrapper around HttpClient that talks to PawTrack.Api.
    // Reads the JWT from the "pt_token" cookie set at login and attaches it
    // as a Bearer token on every call, so callers never think about auth headers.
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public const string TokenCookieName = "pt_token";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AttachToken()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Cookies[TokenCookieName];
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<(bool Success, T? Data, string? Error)> GetAsync<T>(string url)
        {
            AttachToken();
            var response = await _http.GetAsync(url);
            return await ReadResult<T>(response);
        }

        public async Task<(bool Success, T? Data, string? Error)> PostAsync<T>(string url, object? body)
        {
            AttachToken();
            var response = await _http.PostAsJsonAsync(url, body);
            return await ReadResult<T>(response);
        }

        public async Task<(bool Success, string? Error)> PostAsync(string url, object? body)
        {
            AttachToken();
            var response = await _http.PostAsJsonAsync(url, body);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ExtractError(response));
        }

        public async Task<(bool Success, T? Data, string? Error)> PutAsync<T>(string url, object? body)
        {
            AttachToken();
            var response = await _http.PutAsJsonAsync(url, body);
            return await ReadResult<T>(response);
        }

        public async Task<(bool Success, string? Error)> PutAsync(string url, object? body)
        {
            AttachToken();
            var response = await _http.PutAsJsonAsync(url, body);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ExtractError(response));
        }

        public async Task<(bool Success, string? Error)> DeleteAsync(string url)
        {
            AttachToken();
            var response = await _http.DeleteAsync(url);
            if (response.IsSuccessStatusCode) return (true, null);
            return (false, await ExtractError(response));
        }

        private static async Task<(bool, T?, string?)> ReadResult<T>(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return (true, default, null);

                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return (true, data, null);
            }

            return (false, default, await ExtractError(response));
        }

        private static async Task<string> ExtractError(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(raw))
                return $"Request failed ({(int)response.StatusCode} {response.StatusCode}).";

            // API returns either a plain string message or a ProblemDetails-style JSON object
            try
            {
                using var doc = JsonDocument.Parse(raw);
                if (doc.RootElement.ValueKind == JsonValueKind.String)
                    return doc.RootElement.GetString() ?? raw;
                if (doc.RootElement.TryGetProperty("title", out var titleEl))
                    return titleEl.GetString() ?? raw;
                if (doc.RootElement.TryGetProperty("errors", out var errorsEl))
                    return errorsEl.ToString();
            }
            catch (JsonException)
            {
                // not JSON — fall through and return the raw body
            }

            return raw.Trim('"');
        }
    }
}
