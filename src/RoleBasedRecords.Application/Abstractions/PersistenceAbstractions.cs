using System.Linq.Expressions;
using RoleBasedRecords.Application.Records;
using RoleBasedRecords.Domain.Entities;

namespace RoleBasedRecords.Application.Abstractions;

public interface IUserRepository
{
    Task<AppUser?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken);

    Task<bool> IncrementTokenVersionAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IDataRecordReadRepository
{
    Task<IReadOnlyList<DataRecordResponse>> ListAsync(
        Expression<Func<DataRecord, DataRecordResponse>> projection,
        CancellationToken cancellationToken);
}
