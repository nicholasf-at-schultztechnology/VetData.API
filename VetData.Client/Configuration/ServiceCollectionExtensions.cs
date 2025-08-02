using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VetData.Client.Services;
using VetData.Client.Services.Auth;

namespace VetData.Client.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVetDataClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Validate auth configuration
        var authSection = configuration.GetSection(AuthOptions.SectionName);
        var username = authSection["Username"];
        var password = authSection["Password"];

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            throw new InvalidOperationException(
                "VetDataAuth credentials are required. Please configure Username and Password.");
        }

        // Configure options
        services.Configure<AuthOptions>(authSection);
        services.Configure<VetDataClientOptions>(configuration.GetSection(VetDataClientOptions.SectionName));

        // Register HTTP clients
        services.AddHttpClient("VetDataAuth");
        services.AddHttpClient<IVetDataClient, VetDataClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VetDataClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.UserName}:{options.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        });

        // Register services
        services.AddTransient<IAuthenticationService, AuthenticationService>();

        return services;
    }
}