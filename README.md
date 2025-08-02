# VetData.Client

A modern .NET client library for integrating with the VetData/Covetrus API platform. This library provides a strongly-typed, async-first approach to accessing veterinary practice management data through VetData's OData services.

## Installation

```bash
dotnet add package VetData.Client
```

## Quick Start

1. Add authentication configuration to your appsettings.json:
```json
{
  "VetDataAuth": {
    "Username": "your-username",
    "Password": "your-password"
  },
  "VetDataClient": {
    "BaseUrl": "https://api.vetdata.com/",
    "Timeout": "00:00:30",
    "UserName": "your-api-username",
    "Password": "your-api-password"
  }
}
```

2. Register the client in your services:
```csharp
services.AddVetDataClient(configuration);
```

3. Use the client in your code:
```csharp
public class ClientService
{
    private readonly IVetDataClient _client;

    public ClientService(IVetDataClient client)
    {
        _client = client;
    }

    public async Task<IReadOnlyList<ClientRecord>> SearchClientsAsync(
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var searchParams = new ClientSearchParams
        {
            LastName = lastName,
            IncludePhones = true,
            Take = 50
        };

        return await _client.GetClientsAsync(searchParams, cancellationToken);
    }
}
```

## Authentication

The library implements a robust authentication system with the following features:

- Token-based authentication with automatic token refresh
- Rate limiting to prevent API throttling (10 requests per minute)
- Automatic retry on token expiration
- Concurrent request handling with proper locking mechanisms

## Key Components

- **AuthenticationService**: Handles token management and authentication
- **VetDataClient**: Core client implementation for API communication
- **ClientSearchParams**: Strongly-typed search parameters for client queries
- **OData Query Builder**: Type-safe OData query construction
- **Installation Management**: Tools for managing VetData installations
- **Error Handling**: Specialized exceptions and error management

## Common Use Cases

- Retrieving client and patient information
- Searching practice data
- Managing installations
- Synchronizing with practice management systems

## Best Practices

- Always use cancellation tokens for long-running operations
- Implement proper error handling
- Configure appropriate timeout values
- Use pagination for large result sets

## Requirements

- .NET 6.0 or higher
- Valid VetData authentication credentials
- Valid API credentials
- Proper configuration in appsettings.json

## Contributing

Contributions are welcome!

## License

MIT License

## Support

For API access and credentials, contact VetData/Covetrus support.
For library issues, please open a GitHub issue.

## Related Resources

- [VetData API Documentation](https://vetdata.freshdesk.com/support/solutions/articles/8000060016-c-sample-code)
- [Vetdata Support Portal](https://vetdata.freshdesk.com/support/home)