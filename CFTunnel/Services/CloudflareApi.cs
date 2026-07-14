using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CFTunnel.Services
{
    public class CloudflareApi
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "https://api.cloudflare.com/client/v4/";
        private readonly bool _isGlobalKey;

        public CloudflareApi(string apiKey, string? email = null)
        {
            _http = new HttpClient { BaseAddress = new Uri(BaseUrl) };

            if (!string.IsNullOrWhiteSpace(email))
            {
                _http.DefaultRequestHeaders.Add("X-Auth-Key", apiKey);
                _http.DefaultRequestHeaders.Add("X-Auth-Email", email);
                _isGlobalKey = true;
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                _isGlobalKey = false;
            }
        }

        public async Task<bool> VerifyTokenAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                string endpoint = _isGlobalKey ? "user" : "user/tokens/verify";
                var result = await _http.GetFromJsonAsync<CfResult<object>>(endpoint, cancellationToken);
                return result?.Success ?? false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string?> GetZoneIdAsync(string zoneName, CancellationToken cancellationToken = default)
        {
            // Accepts a zone (imtaqin.id) or a full hostname (spse.imtaqin.id):
            // walks up parent domains until a zone matches.
            var candidate = zoneName.Trim().TrimEnd('.');
            while (candidate.Contains('.'))
            {
                try
                {
                    var result = await _http.GetFromJsonAsync<CfResult<CfZone[]?>>($"zones?name={Uri.EscapeDataString(candidate)}", cancellationToken);
                    var id = result?.Result?.FirstOrDefault()?.Id;
                    if (!string.IsNullOrEmpty(id))
                        return id;
                }
                catch
                {
                    return null;
                }
                candidate = candidate[(candidate.IndexOf('.') + 1)..];
            }
            return null;
        }

        public async Task<string?> GetZoneIdFromHostnameAsync(string hostname, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _http.GetFromJsonAsync<CfResult<CfZone[]?>>("zones?per_page=1000", cancellationToken);
                if (result?.Result == null)
                    return null;

                return result.Result
                    .Where(z => IsSubdomain(hostname, z.Name))
                    .OrderByDescending(z => z.Name.Length)
                    .FirstOrDefault()?.Id;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsSubdomain(string hostname, string zoneName)
        {
            if (hostname.Equals(zoneName, StringComparison.OrdinalIgnoreCase))
                return true;
            if (hostname.EndsWith(zoneName, StringComparison.OrdinalIgnoreCase))
            {
                int index = hostname.Length - zoneName.Length - 1;
                return index >= 0 && hostname[index] == '.';
            }
            return false;
        }

        public async Task<bool> CreateCnameAsync(string zoneId, string hostname, string tunnelId, CancellationToken cancellationToken = default)
        {
            try
            {
                var body = new
                {
                    type = "CNAME",
                    name = hostname,
                    content = $"{tunnelId}.cfargotunnel.com",
                    proxied = true,
                    ttl = 1
                };

                var response = await _http.PostAsJsonAsync($"zones/{zoneId}/dns_records", body, cancellationToken);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class CfResult<T>
    {
        public bool Success { get; set; }
        public List<CfApiError> Errors { get; set; } = new();
        public List<object> Messages { get; set; } = new();
        public T? Result { get; set; }
    }

    public class CfApiError
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CfZone
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
