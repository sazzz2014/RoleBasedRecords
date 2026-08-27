using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using RoleBasedRecords.Api.Controllers;
using RoleBasedRecords.Infrastructure.Persistence;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RoleBasedRecords.Api.OpenApi;

public sealed class LoginExampleOperationFilter(
    IHostEnvironment environment,
    IOptions<SeedOptions> seedOptions) : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!environment.IsDevelopment() ||
            context.MethodInfo.DeclaringType != typeof(AuthController) ||
            context.MethodInfo.Name != nameof(AuthController.Login) ||
            operation.RequestBody is not OpenApiRequestBody requestBody ||
            requestBody.Content is null ||
            !requestBody.Content.TryGetValue("application/json", out var mediaType))
        {
            return;
        }

        var settings = seedOptions.Value;
        mediaType.Example = null;
        mediaType.Examples = new Dictionary<string, IOpenApiExample>
        {
            ["Admin"] = CreateExample("Administrator", settings.AdminEmail, settings.AdminPassword),
            ["User"] = CreateExample("Regular user", settings.UserEmail, settings.UserPassword)
        };
    }

    private static OpenApiExample CreateExample(string summary, string email, string password) =>
        new()
        {
            Summary = summary,
            Value = new JsonObject
            {
                ["email"] = email,
                ["password"] = password
            }
        };
}
