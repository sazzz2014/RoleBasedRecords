using System.Security.Claims;
using RoleBasedRecords.Application.Common;
using RoleBasedRecords.Domain.Enums;

namespace RoleBasedRecords.Api.Auth;

public static class CurrentUserExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub");
        return Guid.TryParse(subject, out var userId) && userId != Guid.Empty
            ? userId
            : throw new AppException(AppError.InvalidCredentials, "Invalid credentials");
    }

    public static UserRole GetRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("role") switch
        {
            nameof(UserRole.User) => UserRole.User,
            nameof(UserRole.Admin) => UserRole.Admin,
            _ => throw new AppException(
                AppError.Forbidden,
                "Authenticated user has an unsupported role.")
        };
    }
}
