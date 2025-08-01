namespace VetData.Client.Models;

public record InstallationListRecord(
    Guid InstallationId,
    string PMS,
    string PracticeName);