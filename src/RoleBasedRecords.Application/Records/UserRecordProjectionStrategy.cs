using System.Linq.Expressions;
using RoleBasedRecords.Domain.Entities;
using RoleBasedRecords.Domain.Enums;

namespace RoleBasedRecords.Application.Records;

public sealed class UserRecordProjectionStrategy : IRecordProjectionStrategy
{
    public UserRole Role => UserRole.User;

    public Expression<Func<DataRecord, DataRecordResponse>> Projection =>
        record => new DataRecordResponse
        {
            Id = record.Id,
            Name = record.Name,
            PublicDescription = record.PublicDescription
        };
}
