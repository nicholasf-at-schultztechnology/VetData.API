namespace VetData.Client.Models.SearchParams;

public record ClientSearchParams
{
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public bool IncludePhones { get; init; }
    public int? Skip { get; init; }
    public int? Take { get; init; }
}