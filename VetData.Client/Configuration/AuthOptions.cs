namespace VetData.Client.Configuration;

public class AuthOptions
{
    public const string SectionName = "VetDataAuth";
    public const string GrantType = "http://auth0.com/oauth/grant-type/password-realm";
    public const string ClientId = "vetdata";
    public const string Audience = "https://api.vetdata.com";
    public const string Realm = "vetdata-users";
    public const string ProductCode = "VETDATA-API";
    public const string AuthEndpoint = "https://auth.vetdata.com/oauth/token";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}