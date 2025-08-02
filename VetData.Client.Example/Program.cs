using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VetData.Client.Configuration;
using VetData.Client.Models;
using VetData.Client.Models.SearchParams;
using VetData.Client.Services;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            using var host = CreateHostBuilder(args).Build();
            var app = ActivatorUtilities.CreateInstance<VetDataConsoleApp>(host.Services);
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.ResetColor();
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", optional: false)
                      .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                      .AddEnvironmentVariables()
                      .AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddVetDataClient(context.Configuration);
                services.AddTransient<VetDataConsoleApp>();
            });
}

class VetDataConsoleApp
{
    private readonly IVetDataClient _client;
    private readonly ILogger<VetDataConsoleApp> _logger;

    public VetDataConsoleApp(IVetDataClient client, ILogger<VetDataConsoleApp> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        Console.WriteLine("VetData API Test Client");
        Console.WriteLine("======================");

        while (true)
        {
            Console.WriteLine("\nChoose an operation:");
            Console.WriteLine("1. List Installations");
            Console.WriteLine("2. Search Clients by Last Name");
            Console.WriteLine("3. Search Clients by Email");
            Console.WriteLine("4. Get Client Details with Related Data");
            Console.WriteLine("5. Exit");

            Console.Write("\nEnter your choice (1-5): ");
            var choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1":
                        await ListInstallationsAsync();
                        break;
                    case "2":
                        await SearchClientsByLastNameAsync();
                        break;
                    case "3":
                        await SearchClientsByEmailAsync();
                        break;
                    case "4":
                        await GetClientDetailsAsync();
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                _logger.LogError(ex, "Operation failed");
            }
        }
    }

    private async Task ListInstallationsAsync()
    {
        Console.WriteLine("\nRetrieving installations...");
        var installations = await _client.GetInstallationsAsync();
        
        Console.WriteLine($"\nFound {installations.Count} installation(s):");
        foreach (var install in installations)
        {
            Console.WriteLine($"- {install.PracticeName} ({install.PMS})");
            Console.WriteLine($"  ID: {install.InstallationId}");
        }
    }

    private async Task SearchClientsByLastNameAsync()
    {
        Console.Write("\nEnter last name to search: ");
        var lastName = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(lastName))
        {
            Console.WriteLine("Last name is required.");
            return;
        }

        var searchParams = new ClientSearchParams
        {
            LastName = lastName,
            IncludePhones = true,
            Take = 10
        };

        Console.WriteLine("\nSearching clients...");
        var clients = await _client.GetClientsAsync(searchParams);
        
        DisplayClients(clients);
    }

    private async Task SearchClientsByEmailAsync()
    {
        Console.Write("\nEnter email to search: ");
        var email = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.WriteLine("Email is required.");
            return;
        }

        var searchParams = new ClientSearchParams
        {
            Email = email,
            IncludePhones = true,
            Take = 10
        };

        Console.WriteLine("\nSearching clients...");
        var clients = await _client.GetClientsAsync(searchParams);
        
        DisplayClients(clients);
    }

    private async Task GetClientDetailsAsync()
    {
        Console.Write("\nEnter client Account ID: ");
        var accountId = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(accountId))
        {
            Console.WriteLine("Account ID is required.");
            return;
        }

        var searchParams = new ClientSearchParams
        {
            LastName = accountId,  // Using LastName field for AccountId search (adjust based on actual API capabilities)
            IncludePhones = true,
            Take = 1
        };

        Console.WriteLine("\nRetrieving client details...");
        var clients = await _client.GetClientsAsync(searchParams);
        
        DisplayClients(clients, detailed: true);
    }

    private void DisplayClients(IReadOnlyList<ClientRecord> clients, bool detailed = false)
    {
        Console.WriteLine($"\nFound {clients.Count} client(s):");
        
        foreach (var client in clients)
        {
            Console.WriteLine($"\n- {client.FirstName} {client.LastName}");
            Console.WriteLine($"  Account ID: {client.AccountId}");
            
            if (client.Phones.Any())
            {
                Console.WriteLine("  Phone Numbers:");
                foreach (var phone in client.Phones)
                {
                    Console.WriteLine($"    {phone.PhoneType}: {phone.PhoneNumber}");
                }
            }

            if (detailed)
            {
                Console.WriteLine($"  Created: {client.APICreateDate}");
                Console.WriteLine($"  Last Modified: {client.APILastChangeDate}");
            }
        }
    }
}