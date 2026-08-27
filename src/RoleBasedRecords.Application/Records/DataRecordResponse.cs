namespace RoleBasedRecords.Application.Records;

public sealed class DataRecordResponse
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public required string PublicDescription { get; init; }

    public string? InternalComment { get; init; }

    public decimal? CostPrice { get; init; }

    public Guid? CreatedByUserId { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }
}
