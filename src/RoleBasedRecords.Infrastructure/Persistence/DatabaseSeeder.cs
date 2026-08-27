using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Domain.Entities;
using RoleBasedRecords.Domain.Enums;

namespace RoleBasedRecords.Infrastructure.Persistence;

public sealed class DatabaseSeeder(
    AppDbContext dbContext,
    IPasswordService passwordService,
    IOptions<SeedOptions> options,
    TimeProvider timeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var admin = await SeedAccountAsync(
            settings.AdminEmail,
            settings.AdminPassword,
            UserRole.Admin,
            $"{SeedOptions.SectionName}:Admin",
            cancellationToken);

        await SeedAccountAsync(
            settings.UserEmail,
            settings.UserPassword,
            UserRole.User,
            $"{SeedOptions.SectionName}:User",
            cancellationToken);

        if (await dbContext.DataRecords.AnyAsync(cancellationToken))
        {
            return;
        }

        dbContext.DataRecords.Add(new DataRecord
        {
            Id = Guid.NewGuid(),
            Name = "Demo record",
            PublicDescription = "Visible to User and Admin roles.",
            InternalComment = "Visible only to Admin.",
            CostPrice = 42.50m,
            CreatedByUserId = admin.Id,
            CreatedAt = timeProvider.GetUtcNow()
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AppUser> SeedAccountAsync(
        string configuredEmail,
        string password,
        UserRole role,
        string sectionName,
        CancellationToken cancellationToken)
    {
        var email = configuredEmail.Trim();

        if (string.IsNullOrWhiteSpace(email) ||
            !new EmailAddressAttribute().IsValid(email) ||
            string.IsNullOrWhiteSpace(password) ||
            password.Length is < 12 or > 128)
        {
            throw new InvalidOperationException(
                $"{sectionName} requires a valid email and a password between 12 and 128 characters.");
        }

        var normalizedEmail = email.ToUpperInvariant();
        var account = await dbContext.Users.SingleOrDefaultAsync(
            user => user.NormalizedEmail == normalizedEmail,
            cancellationToken);

        if (account is not null)
        {
            account.Email = email;
            account.Role = role;
            account.IsActive = true;

            if (!passwordService.VerifyPassword(account, account.PasswordHash, password))
            {
                account.PasswordHash = passwordService.HashPassword(account, password);
                account.TokenVersion++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            return account;
        }

        account = new AppUser
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            Role = role,
            TokenVersion = 0,
            IsActive = true,
            CreatedAt = timeProvider.GetUtcNow()
        };
        account.PasswordHash = passwordService.HashPassword(account, password);
        dbContext.Users.Add(account);
        await dbContext.SaveChangesAsync(cancellationToken);

        return account;
    }
}
