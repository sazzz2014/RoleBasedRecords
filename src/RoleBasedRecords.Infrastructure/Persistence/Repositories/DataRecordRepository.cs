using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Application.Records;
using RoleBasedRecords.Domain.Entities;

namespace RoleBasedRecords.Infrastructure.Persistence.Repositories;

public sealed class DataRecordRepository(AppDbContext dbContext) : IDataRecordReadRepository
{
    public async Task<IReadOnlyList<DataRecordResponse>> ListAsync(
        Expression<Func<DataRecord, DataRecordResponse>> projection,
        CancellationToken cancellationToken)
    {
        return await dbContext.DataRecords
            .AsNoTracking()
            .OrderByDescending(record => record.CreatedAt)
            .Take(100)
            .Select(projection)
            .ToListAsync(cancellationToken);
    }
}
