using System.Security.Claims;
using AegiFinance.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromQuery] string? returnUrl = null)
    {
        try
        {
            var authResult = await _authService.LoginAsync(username, password);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, authResult.User.Id.ToString()),
                new Claim(ClaimTypes.Name, authResult.User.Name),
                new Claim(ClaimTypes.Email, authResult.User.Email),
                new Claim("UserType", authResult.User.UserType)
            };

            if (authResult.User.ClientId.HasValue)
            {
                claims.Add(new Claim("ClientId", authResult.User.ClientId.Value.ToString()));
            }

            foreach (var role in authResult.User.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var permission in authResult.User.Permissions)
            {
                claims.Add(new Claim("Permission", permission));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddDays(7)
            });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }
        catch (UnauthorizedAccessException)
        {
            return Redirect("/login?error=invalid_credentials");
        }
        catch (Exception)
        {
            return Redirect("/login?error=server_error");
        }
    }

    [HttpPost("logout")]
    [HttpGet("logout")] // Allow GET for easier logout from UI buttons
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
