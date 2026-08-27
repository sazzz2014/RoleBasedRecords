namespace RoleBasedRecords.Domain.Entities;

public sealed class DataRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PublicDescription { get; set; } = string.Empty;

    public string? InternalComment { get; set; }

    public decimal? CostPrice { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
