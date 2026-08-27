using System.Linq.Expressions;
using RoleBasedRecords.Domain.Entities;
using RoleBasedRecords.Domain.Enums;

namespace RoleBasedRecords.Application.Records;

public sealed class AdminRecordProjectionStrategy : IRecordProjectionStrategy
{
    public UserRole Role => UserRole.Admin;

    public Expression<Func<DataRecord, DataRecordResponse>> Projection =>
        record => new DataRecordResponse
        {
            Id = record.Id,
            Name = record.Name,
            PublicDescription = record.PublicDescription,
            InternalComment = record.InternalComment,
            CostPrice = record.CostPrice,
            CreatedByUserId = record.CreatedByUserId,
            CreatedAt = record.CreatedAt
        };
}
