using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RoleBasedRecords.Domain.Enums;
using RoleBasedRecords.Infrastructure.Auth;

namespace RoleBasedRecords.Api.Auth;

public sealed class JwtBearerConfiguration(
    IOptions<JwtOptions> jwtOptions,
    ILogger<JwtBearerConfiguration> logger) :
    IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(JwtBearerOptions options)
    {
        Configure(JwtBearerDefaults.AuthenticationScheme, options);
    }

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name != JwtBearerDefaults.AuthenticationScheme)
        {
            return;
        }

        var settings = jwtOptions.Value;

        options.MapInboundClaims = false;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = settings.CreateSigningKey(),
            RequireSignedTokens = true,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = "role",
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ValidateTokenStateAsync
        };
    }

    private async Task ValidateTokenStateAsync(TokenValidatedContext context)
    {
        var principal = context.Principal;
        var subject = principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var roleValue = principal?.FindFirst("role")?.Value;
        var versionValue = principal?.FindFirst("ver")?.Value;

        if (!Guid.TryParse(subject, out var userId) ||
            userId == Guid.Empty ||
            !TryParseRole(roleValue, out var role) ||
            !long.TryParse(
                versionValue,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var tokenVersion) ||
            tokenVersion < 0)
        {
            context.Fail("Required token claims are missing or invalid.");
            return;
        }

        try
        {
            var validator = context.HttpContext.RequestServices
                .GetRequiredService<TokenStateValidator>();

            var isValid = await validator.IsValidAsync(
                userId,
                role,
                tokenVersion,
                context.HttpContext.RequestAborted);

            if (!isValid)
            {
                context.Fail("Token state is no longer valid.");
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Token state validation failed.");
            context.Fail("Token state could not be validated.");
        }
    }

    private static bool TryParseRole(string? value, out UserRole role)
    {
        switch (value)
        {
            case nameof(UserRole.User):
                role = UserRole.User;
                return true;
            case nameof(UserRole.Admin):
                role = UserRole.Admin;
                return true;
            default:
                role = default;
                return false;
        }
    }
}
