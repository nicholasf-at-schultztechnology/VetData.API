namespace VetData.Client.Models;

public record ClientRecord
{
    public string AccountId { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public IReadOnlyList<PhoneRecord> Phones { get; init; } = Array.Empty<PhoneRecord>();
    public DateTime? APICreateDate { get; init; }
    public DateTime? APILastChangeDate { get; init; }
}