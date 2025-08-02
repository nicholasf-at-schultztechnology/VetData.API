using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VetData.Client.Configuration;
using VetData.Client.Exceptions;
using VetData.Client.Models;

namespace VetData.Client.Services.Auth;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly AuthOptions _authOptions;
    private readonly ILogger<AuthenticationService> _logger;
    private TokenResponse? _currentToken;
    private DateTime _tokenExpiration = DateTime.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    
    private readonly Queue<DateTime> _tokenRequests = new();
    private const int MaxRequestsPerMinute = 10;
    private const int RequestsWindowSeconds = 60;

    public AuthenticationService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthOptions> authOptions,
        ILogger<AuthenticationService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("VetDataAuth");
        _authOptions = authOptions.Value;
        _logger = logger;
        ServicePointManager.DefaultConnectionLimit = 100;
    }

    private void EnforceRateLimit()
    {
        lock (_tokenRequests)
        {
            while (_tokenRequests.Count > 0 && 
                   DateTime.UtcNow.Subtract(_tokenRequests.Peek()).TotalSeconds > RequestsWindowSeconds)
            {
                _tokenRequests.Dequeue();
            }

            if (_tokenRequests.Count >= MaxRequestsPerMinute)
            {
                var oldestRequest = _tokenRequests.Peek();
                var waitTime = RequestsWindowSeconds - 
                    (int)DateTime.UtcNow.Subtract(oldestRequest).TotalSeconds;
                
                if (waitTime > 0)
                {
                    throw new VetDataException(
                        $"Rate limit exceeded. Please wait {waitTime} seconds before retrying.");
                }
            }

            _tokenRequests.Enqueue(DateTime.UtcNow);
        }
    }

    public async Task<string> GetAccessTokenAsync(
        CancellationToken cancellationToken = default)
    {
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            if (_currentToken != null && 
                DateTime.UtcNow < _tokenExpiration.AddMinutes(-5))
            {
                return _currentToken.AccessToken;
            }

            EnforceRateLimit();

            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = AuthOptions.GrantType,
                ["client_id"] = AuthOptions.ClientId,
                ["audience"] = AuthOptions.Audience,
                ["username"] = _authOptions.Username,
                ["password"] = _authOptions.Password,
                ["realm"] = AuthOptions.Realm,
                ["covetrus_product_code"] = AuthOptions.ProductCode
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, 
                AuthOptions.AuthEndpoint)
            {
                Content = content
            };
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient
                .SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new VetDataAuthenticationException(
                    "Invalid credentials or authentication failed");
            }

            response.EnsureSuccessStatusCode();

            _currentToken = await response.Content
                .ReadFromJsonAsync<TokenResponse>(
                    cancellationToken: cancellationToken);
            
            if (_currentToken == null)
            {
                throw new VetDataException("Failed to deserialize token response");
            }

            _tokenExpiration = DateTime.UtcNow.AddSeconds(_currentToken.ExpiresIn);
            
            return _currentToken.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to obtain access token");
            throw new VetDataAuthenticationException(
                "Failed to authenticate with the server", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to obtain access token");
            throw new VetDataException("Authentication failed", ex);
        }
        finally
        {
            _tokenLock.Release();
        }
    }
}