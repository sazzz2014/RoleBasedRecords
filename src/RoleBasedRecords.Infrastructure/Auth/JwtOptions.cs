using Microsoft.IdentityModel.Tokens;

namespace RoleBasedRecords.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKeyBase64 { get; init; } = string.Empty;

    public int AccessTokenMinutes { get; init; } = 15;

    public SymmetricSecurityKey CreateSigningKey() =>
        new(Convert.FromBase64String(SigningKeyBase64));
}
