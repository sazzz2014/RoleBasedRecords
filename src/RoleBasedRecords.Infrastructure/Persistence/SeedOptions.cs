namespace RoleBasedRecords.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public string AdminEmail { get; init; } = string.Empty;

    public string AdminPassword { get; init; } = string.Empty;

    public string UserEmail { get; init; } = string.Empty;

    public string UserPassword { get; init; } = string.Empty;
}
