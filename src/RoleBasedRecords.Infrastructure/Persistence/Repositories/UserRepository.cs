using Microsoft.EntityFrameworkCore;
using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Domain.Entities;

namespace RoleBasedRecords.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public Task<AppUser?> FindByNormalizedEmailAsync(
        string normalizedEmail,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                user => user.NormalizedEmail == normalizedEmail,
                cancellationToken);
    }

    public async Task<bool> IncrementTokenVersionAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.Users
            .Where(user => user.Id == userId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    user => user.TokenVersion,
                    user => user.TokenVersion + 1),
                cancellationToken);

        return affectedRows == 1;
    }
}
