using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using VetData.Client.Services;

namespace VetData.Client.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddVetDataClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<VetDataClientOptions>(configuration.GetSection(VetDataClientOptions.SectionName));

        services.AddHttpClient<IVetDataClient, VetDataClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<VetDataClientOptions>>().Value;

            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.UserName}:{options.Password}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
        });

        return services;
    }
}