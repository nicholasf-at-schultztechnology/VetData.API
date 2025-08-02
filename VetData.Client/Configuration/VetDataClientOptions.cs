namespace VetData.Client.Configuration;

public class VetDataClientOptions
{
    public const string SectionName = "VetDataClient";
    
    public string BaseUrl { get; set; } = "https://api.vetdata.com/";
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}