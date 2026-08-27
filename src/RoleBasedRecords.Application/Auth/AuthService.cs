using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Application.Common;

namespace RoleBasedRecords.Application.Auth;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService)
{
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await userRepository.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            throw new AppException(AppError.InvalidCredentials, "Invalid credentials");
        }

        var token = jwtTokenService.CreateToken(user);
        return new LoginResponse(token.Value, "Bearer", token.ExpiresAt);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (!await userRepository.IncrementTokenVersionAsync(userId, cancellationToken))
        {
            throw new AppException(AppError.NotFound, "User was not found.");
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToUpperInvariant();
}
