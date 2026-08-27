using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RoleBasedRecords.Api.Auth;
using RoleBasedRecords.Application.Auth;
using RoleBasedRecords.Application.Common;

namespace RoleBasedRecords.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    AuthService authService,
    ILogger<AuthController> logger) : ControllerBase
{
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.LoginAsync(request, cancellationToken));
        }
        catch (AppException exception) when (exception.Error == AppError.InvalidCredentials)
        {
            logger.LogWarning(
                "Unsuccessful login attempt from {RemoteIpAddress}.",
                HttpContext.Connection.RemoteIpAddress);
            throw;
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
