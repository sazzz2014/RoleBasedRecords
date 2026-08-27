using System.ComponentModel.DataAnnotations;

namespace RoleBasedRecords.Application.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, StringLength(128, MinimumLength = 1)] string Password);

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt);

public sealed record IssuedToken(string Value, DateTimeOffset ExpiresAt);
