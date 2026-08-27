using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Application.Common;
using RoleBasedRecords.Domain.Enums;

namespace RoleBasedRecords.Application.Records;

public sealed class RecordService(
    IDataRecordReadRepository readRepository,
    IEnumerable<IRecordProjectionStrategy> strategies)
{
    private readonly IReadOnlyDictionary<UserRole, IRecordProjectionStrategy> _strategies =
        strategies.ToDictionary(strategy => strategy.Role);

    public Task<IReadOnlyList<DataRecordResponse>> ListAsync(
        UserRole role,
        CancellationToken cancellationToken)
    {
        if (!_strategies.TryGetValue(role, out var strategy))
        {
            throw new AppException(AppError.Forbidden, $"Unsupported role: {role}");
        }

        return readRepository.ListAsync(strategy.Projection, cancellationToken);
    }
}
