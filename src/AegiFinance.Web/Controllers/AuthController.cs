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
            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
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
        await _mediator.Send(new LogoutCommand(User.GetUserId()), cancellationToken);
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
}
