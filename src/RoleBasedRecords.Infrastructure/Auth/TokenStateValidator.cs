using Microsoft.EntityFrameworkCore;
using RoleBasedRecords.Domain.Enums;
using RoleBasedRecords.Infrastructure.Persistence;

namespace RoleBasedRecords.Infrastructure.Auth;

public sealed class TokenStateValidator(AppDbContext dbContext)
{
    public Task<bool> IsValidAsync(
        Guid userId,
        UserRole role,
        long tokenVersion,
        CancellationToken cancellationToken)
    {
        return dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user =>
                    user.Id == userId &&
                    user.IsActive &&
                    user.Role == role &&
                    user.TokenVersion == tokenVersion,
                cancellationToken);
    }
}
