namespace VetData.Client.Configuration;

public class VetDataClientOptions
{
    public const string SectionName = "VetData";
    
    public string BaseUrl { get; set; } = "https://api.vetdata.net/v2";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string InstallationId { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}