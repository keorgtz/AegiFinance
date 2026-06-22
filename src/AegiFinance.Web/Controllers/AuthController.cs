using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request.UsernameOrEmail, request.Password, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login-pin")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> LoginWithPin(LoginPinRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginWithPinAsync(request.Username, request.Pin, cancellationToken);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _authService.LogoutAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        return Ok(result);
    }
}

public record LoginRequest(string UsernameOrEmail, string Password);
public record LoginPinRequest(string Username, string Pin);
public record RefreshRequest(string RefreshToken);
