using AegiFinance.Application.Dtos;
using AegiFinance.Application.Features.Auth.Commands.ChangePassword;
using AegiFinance.Application.Features.Auth.Commands.Login;
using AegiFinance.Application.Features.Auth.Commands.LoginWithPin;
using AegiFinance.Application.Features.Auth.Commands.Logout;
using AegiFinance.Application.Features.Auth.Commands.RefreshToken;
using AegiFinance.Application.Features.Auth.Queries.GetCurrentUser;
using AegiFinance.Web.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "AegiFinance.RefreshToken";

    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginCommand command, CancellationToken cancellationToken)
    {
        ApplySessionContext(command);
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiry);
            return Ok(new AuthResponseDto { AccessToken = result.AccessToken, User = result.User });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("login-pin")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> LoginWithPin(LoginWithPinCommand command, CancellationToken cancellationToken)
    {
        ApplySessionContext(command);
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiry);
            return Ok(new AuthResponseDto { AccessToken = result.AccessToken, User = result.User });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "No se encontró un refresh token." });
        }

        try
        {
            var command = new RefreshTokenCommand(refreshToken);
            ApplySessionContext(command);
            var result = await _mediator.Send(command, cancellationToken);
            SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiry);
            return Ok(new AuthResponseDto { AccessToken = result.AccessToken, User = result.User });
        }
        catch (UnauthorizedAccessException ex)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Guid? sessionId = Guid.TryParse(User.FindFirst("sessionId")?.Value, out var parsedSessionId) ? parsedSessionId : null;
        await _mediator.Send(new LogoutCommand(User.GetUserId(), sessionId), cancellationToken);
        ClearRefreshTokenCookie();
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        command.UserId = User.GetUserId();

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me(CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _mediator.Send(new GetCurrentUserQuery(), cancellationToken));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiry)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = expiry,
            Path = "/api/auth"
        });
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Path = "/api/auth"
        });
    }

    private void ApplySessionContext(LoginCommand command)
    {
        command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        command.UserAgent = Request.Headers.UserAgent.ToString();
        command.DeviceName = DescribeDevice(command.UserAgent);
    }

    private void ApplySessionContext(LoginWithPinCommand command)
    {
        command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        command.UserAgent = Request.Headers.UserAgent.ToString();
        command.DeviceName = DescribeDevice(command.UserAgent);
    }

    private void ApplySessionContext(RefreshTokenCommand command)
    {
        command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        command.UserAgent = Request.Headers.UserAgent.ToString();
        command.DeviceName = DescribeDevice(command.UserAgent);
    }

    private static string DescribeDevice(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent)) return "Dispositivo desconocido";
        var platform = userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase) ? "Teléfono" : "Computadora";
        var browser = userAgent.Contains("Edg/", StringComparison.OrdinalIgnoreCase) ? "Edge" :
            userAgent.Contains("Chrome/", StringComparison.OrdinalIgnoreCase) ? "Chrome" :
            userAgent.Contains("Firefox/", StringComparison.OrdinalIgnoreCase) ? "Firefox" :
            userAgent.Contains("Safari/", StringComparison.OrdinalIgnoreCase) ? "Safari" : "Navegador";
        return $"{platform} · {browser}";
    }
}
