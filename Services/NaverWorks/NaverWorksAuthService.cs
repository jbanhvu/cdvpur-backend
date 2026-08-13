using ChangdaeVinaPurchasingApi.Models.NaverWorks;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChangdaeVinaPurchasingApi.Services.NaverWorks;

public class NaverWorksAuthService
{
    private const string TokenUrl = "https://auth.worksmobile.com/oauth2/v2.0/token";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHostEnvironment _environment;
    private readonly IOptions<NaverWorksOptions> _options;
    private readonly ILogger<NaverWorksAuthService> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiresAt;

    public NaverWorksAuthService(
        IHttpClientFactory httpClientFactory,
        IHostEnvironment environment,
        IOptions<NaverWorksOptions> options,
        ILogger<NaverWorksAuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _environment = environment;
        _options = options;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) &&
            _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
        {
            return _accessToken;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) &&
                _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return _accessToken;
            }

            NaverWorksOptions options = GetValidatedOptions();
            string assertion = CreateJwt(options);

            using HttpRequestMessage request = new(HttpMethod.Post, TokenUrl)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                    ["client_id"] = options.ClientId,
                    ["client_secret"] = options.ClientSecret,
                    ["assertion"] = assertion,
                    ["scope"] = options.Scope
                })
            };

            HttpClient client = _httpClientFactory.CreateClient();
            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("NAVER WORKS token API returned HTTP {StatusCode}.", (int)response.StatusCode);
                throw new InvalidOperationException($"NAVER WORKS token API returned HTTP {(int)response.StatusCode}: {responseBody}");
            }

            NaverWorksTokenResponse? token = JsonSerializer.Deserialize<NaverWorksTokenResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (token is null || string.IsNullOrWhiteSpace(token.AccessToken))
            {
                throw new InvalidOperationException("NAVER WORKS token API did not return access_token.");
            }

            _accessToken = token.AccessToken;
            int expiresIn = ReadExpiresIn(token.ExpiresIn);
            _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(expiresIn - 60, 60));

            return _accessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public void ClearAccessToken()
    {
        _accessToken = null;
        _accessTokenExpiresAt = DateTimeOffset.MinValue;
    }

    private NaverWorksOptions GetValidatedOptions()
    {
        NaverWorksOptions options = _options.Value;

        if (string.IsNullOrWhiteSpace(options.ClientId) ||
            string.IsNullOrWhiteSpace(options.ClientSecret) ||
            string.IsNullOrWhiteSpace(options.ServiceAccount) ||
            string.IsNullOrWhiteSpace(options.PrivateKeyPath) ||
            string.IsNullOrWhiteSpace(options.Scope))
        {
            throw new InvalidOperationException("NaverWorks configuration is incomplete.");
        }

        return options;
    }

    private string CreateJwt(NaverWorksOptions options)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        object header = new { alg = "RS256", typ = "JWT" };
        object payload = new
        {
            iss = options.ClientId,
            sub = options.ServiceAccount,
            iat = now.ToUnixTimeSeconds(),
            exp = now.AddMinutes(30).ToUnixTimeSeconds()
        };

        string unsignedToken = $"{Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header))}.{Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload))}";
        string privateKey = ReadPrivateKey(options.PrivateKeyPath);

        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(privateKey);

        byte[] signature = rsa.SignData(
            Encoding.UTF8.GetBytes(unsignedToken),
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private string ReadPrivateKey(string privateKeyPath)
    {
        string path = Path.IsPathRooted(privateKeyPath)
            ? privateKeyPath
            : Path.Combine(_environment.ContentRootPath, privateKeyPath);

        if (!File.Exists(path))
        {
            throw new InvalidOperationException($"NAVER WORKS private key file was not found: {privateKeyPath}");
        }

        return File.ReadAllText(path).Replace("\\n", "\n", StringComparison.Ordinal);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static int ReadExpiresIn(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int number) && number > 0)
        {
            return number;
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), out number) &&
            number > 0)
        {
            return number;
        }

        return 3600;
    }
}
