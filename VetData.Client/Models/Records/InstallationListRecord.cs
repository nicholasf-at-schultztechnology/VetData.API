namespace VetData.Client.Models;

public record InstallationListRecord
{
    public Guid InstallationId { get; init; }
    public string PMS { get; init; } = string.Empty;
    public string PracticeName { get; init; } = string.Empty;
}