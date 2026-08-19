using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AegiFinance.Web.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Policy = "ViewSettings")]
public sealed class SettingsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { module = "settings", status = "available" });
}
