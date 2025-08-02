namespace VetData.Client.Models;

public record PhoneRecord
{
    public string PhoneNumber { get; init; } = string.Empty;
    public string PhoneType { get; init; } = string.Empty;
}
