using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using RoleBasedRecords.Api.Auth;
using RoleBasedRecords.Api.ErrorHandling;
using RoleBasedRecords.Api.OpenApi;
using RoleBasedRecords.Application.Abstractions;
using RoleBasedRecords.Application.Auth;
using RoleBasedRecords.Application.Records;
using RoleBasedRecords.Infrastructure.Auth;
using RoleBasedRecords.Infrastructure.Persistence;
using RoleBasedRecords.Infrastructure.Persistence.Repositories;

namespace RoleBasedRecords.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoleBasedRecords(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition =
                    System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddSingleton(TimeProvider.System);

        AddApplication(services);
        AddInfrastructure(services, configuration);
        AddSecurity(services);

        services.AddSwaggerGen(options =>
        {
            const string schemeName = JwtBearerDefaults.AuthenticationScheme;

            options.AddSecurityDefinition(
                schemeName,
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter the access token returned by POST /api/auth/login."
                });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(schemeName, document, null)] = []
            });

            options.OperationFilter<LoginExampleOperationFilter>();
        });

        return services;
    }

    private static void AddApplication(IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<RecordService>();
        services.AddScoped<IRecordProjectionStrategy, UserRecordProjectionStrategy>();
        services.AddScoped<IRecordProjectionStrategy, AdminRecordProjectionStrategy>();
    }

    private static void AddInfrastructure(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException(
                "ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDataRecordReadRepository, DataRecordRepository>();

        services.AddSingleton<IPasswordService, AspNetPasswordService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<TokenStateValidator>();
        services.AddScoped<DatabaseSeeder>();

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();

        services
            .AddOptions<SeedOptions>()
            .Bind(configuration.GetSection(SeedOptions.SectionName));
    }

    private static void AddSecurity(IServiceCollection services)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerConfiguration>();

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build());

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("login", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(60),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });
    }
}
