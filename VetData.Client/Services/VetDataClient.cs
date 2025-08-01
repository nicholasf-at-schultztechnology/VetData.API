
namespace VetData.Client.Services;

public class VetDataClient : IVetDataClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<VetDataClient> _logger;
    private readonly VetDataClientOptions _options;

    public VetDataClient(
        HttpClient httpClient,
        IOptions<VetDataClientOptions> options,
        ILogger<VetDataClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<InstallationListRecord>> GetInstallationsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                "InstallationList", 
                cancellationToken);
            
            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<InstallationListRecord>>(
                    cancellationToken: cancellationToken);

            return result ?? Array.Empty<InstallationListRecord>();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving installations");
            throw new VetDataException("Failed to retrieve installations", ex);
        }
    }

    public async Task<IReadOnlyList<ClientRecord>> GetClientsAsync(
        ClientSearchParams searchParams,
        CancellationToken cancellationToken = default)
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

        if (searchParams.IncludePhones)
            queryBuilder.AddExpand("Phones,ClientPatientRelationships/Patient");

        if (searchParams.Skip.HasValue)
            queryBuilder.AddSkip(searchParams.Skip.Value);

        if (searchParams.Take.HasValue)
            queryBuilder.AddTop(searchParams.Take.Value);

        return queryBuilder.Build();
    }
}