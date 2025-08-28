using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VetData.Client.Models;
using VetData.Client.Configuration;
using VetData.Client.Models.SearchParams;
using VetData.Client.Helpers;
using VetData.Client.Exceptions;
using VetData.Client.Models.Responses;
using System.Net.Http.Json;
using System.Net;
using System.Net.Http.Headers;
using VetData.Client.Services.Auth;

namespace VetData.Client.Services;

public class VetDataClient : IVetDataClient
{
    private readonly HttpClient _httpClient;
    private readonly IAuthenticationService _authService;
    private readonly ILogger<VetDataClient> _logger;
    private const int MaxRetries = 1;

    public VetDataClient(
        HttpClient httpClient,
        IOptions<VetDataClientOptions> options,
        IAuthenticationService authService,
        ILogger<VetDataClient> logger)
    {
        _httpClient = httpClient;
        _authService = authService;
        _logger = logger;
    }

    private async Task<T> ExecuteWithRetryAsync<T>(
        Func<string, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                var token = await _authService
                    .GetAccessTokenAsync(cancellationToken);
                return await operation(token);
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Unauthorized
                      && attempts < MaxRetries)
            {
                attempts++;
                _logger.LogWarning(
                    "Token expired during request, attempting retry {Attempt} of {MaxRetries}",
                    attempts, MaxRetries);
                continue;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Request failed");
                throw;
            }
        }
    }

    public async Task<IReadOnlyList<InstallationListRecord>> GetInstallationsAsync(
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithRetryAsync(async (token) =>
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, "InstallationList");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient
                .SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<InstallationListRecord>>(
                    cancellationToken: cancellationToken);

            return result ?? Array.Empty<InstallationListRecord>();
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<ClientRecord>> GetClientsAsync(ClientSearchParams searchParams, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildODataQuery(searchParams);
            var response = await _httpClient.GetAsync(
                $"Clients{query}",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<ODataResponse<ClientRecord>>(
                    cancellationToken: cancellationToken);

            return result?.Value ?? Array.Empty<ClientRecord>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving clients");
            throw new VetDataException("Failed to retrieve clients", ex);
        }
    }

    private string BuildODataQuery(ClientSearchParams searchParams)
    {
        var queryBuilder = new ODataQueryBuilder();

        if (!string.IsNullOrEmpty(searchParams.LastName))
            queryBuilder.AddFilter($"contains(LastName,'{searchParams.LastName}')");

        if (!string.IsNullOrEmpty(searchParams.Email))
            queryBuilder.AddFilter($"Emails/any(e: contains(e.Address,'{searchParams.Email}'))");

        // Below may not actually work - needs testing with real data
        if (!string.IsNullOrEmpty(searchParams.Phone))
            queryBuilder.AddFilter($"Phones/any(p: contains(p.PhoneNumber,'{searchParams.Phone}'))");

        if (searchParams.IncludePhones)
            queryBuilder.AddExpand("Phones,ClientPatientRelationships/Patient");

        if (searchParams.Skip.HasValue)
            queryBuilder.AddSkip(searchParams.Skip.Value);

        if (searchParams.Take.HasValue)
            queryBuilder.AddTop(searchParams.Take.Value);

        return queryBuilder.Build();
    }
}