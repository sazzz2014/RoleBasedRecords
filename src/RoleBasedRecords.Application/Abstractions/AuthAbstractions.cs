using RoleBasedRecords.Application.Auth;
using RoleBasedRecords.Domain.Entities;

namespace RoleBasedRecords.Application.Abstractions;

public interface IPasswordService
{
    string HashPassword(AppUser user, string password);

    bool VerifyPassword(AppUser user, string passwordHash, string providedPassword);
}

public interface IJwtTokenService
{
    IssuedToken CreateToken(AppUser user);
}
