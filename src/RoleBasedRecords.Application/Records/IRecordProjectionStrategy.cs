using System.Linq.Expressions;
using RoleBasedRecords.Domain.Entities;
using RoleBasedRecords.Domain.Enums;

namespace RoleBasedRecords.Application.Records;

public interface IRecordProjectionStrategy
{
    UserRole Role { get; }

    Expression<Func<DataRecord, DataRecordResponse>> Projection { get; }
}
