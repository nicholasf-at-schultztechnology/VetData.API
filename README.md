Example usage in a consuming application:

```Csharp
// Program.cs or Startup.cs
services.AddVetDataClient(configuration);

// appsettings.json
{
  "VetData": {
    "BaseUrl": "https://api.vetdata.net/v2",
    "UserName": "your-username",
    "Password": "your-password",
    "InstallationId": "your-installation-id",
    "Timeout": "00:00:30"
  }
}

// Example service using the client
public class ClientService
{
    private readonly IVetDataClient _client;

    public ClientService(IVetDataClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<ClientRecord>> SearchClientsAsync(
        string lastName,
        bool includePhones = false,
        CancellationToken cancellationToken = default)
    {
        var searchParams = new ClientSearchParams
        {
            LastName = lastName,
            IncludePhones = includePhones,
            Take = 50
        };

        return await _client.GetClientsAsync(searchParams, cancellationToken);
    }
}
```