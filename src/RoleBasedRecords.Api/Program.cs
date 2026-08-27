using Microsoft.EntityFrameworkCore;
using RoleBasedRecords.Api.Extensions;
using RoleBasedRecords.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRoleBasedRecords(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
if (builder.Configuration.GetValue("HttpsRedirection:Enabled", true))
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnableTryItOutByDefault();
        options.UseResponseInterceptor(
            "(response) => { const url = new URL(response.url, window.location.origin); " +
            "if (!url.pathname.endsWith('/api/auth/login') || response.status !== 200) " +
            "{ return response; } if (response.obj?.accessToken) " +
            "{ window.ui.preauthorizeApiKey('Bearer', response.obj.accessToken); } " +
            "return response; }");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    if (app.Environment.IsDevelopment())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(CancellationToken.None);
    }
}

await app.RunAsync();
