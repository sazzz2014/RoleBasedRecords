using Microsoft.Extensions.Options;

namespace RoleBasedRecords.Infrastructure.Auth;

public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("Jwt:Issuer is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("Jwt:Audience is required.");
        }

        if (options.AccessTokenMinutes is < 1 or > 60)
        {
            return ValidateOptionsResult.Fail(
                "Jwt:AccessTokenMinutes must be between 1 and 60.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKeyBase64))
        {
            return ValidateOptionsResult.Fail("Jwt:SigningKeyBase64 is required.");
        }

        try
        {
            var key = Convert.FromBase64String(options.SigningKeyBase64);
            if (key.Length < 32)
            {
                return ValidateOptionsResult.Fail(
                    "Jwt:SigningKeyBase64 must decode to at least 32 random bytes.");
            }
        }
        catch (FormatException)
        {
            return ValidateOptionsResult.Fail("Jwt:SigningKeyBase64 is not valid Base64.");
        }

        return ValidateOptionsResult.Success;
    }
}
