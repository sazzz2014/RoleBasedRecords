using Microsoft.AspNetCore.Identity;
using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Domain.Entities;

namespace RoleBasedRecords.Infrastructure.Auth;

public sealed class AspNetPasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public string HashPassword(AppUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(
        AppUser user,
        string passwordHash,
        string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            providedPassword);

        return result != PasswordVerificationResult.Failed;
    }
}
