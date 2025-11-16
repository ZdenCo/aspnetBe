using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProperAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // GET /api/users/getUser
    [HttpGet("getUser")]
    [Authorize] // requires valid Bearer token
    public IActionResult GetUser()
    {
        var user = HttpContext.User;

        // Typical JWT claims: sub (user id), email
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("sub")?.Value;

        var email = user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.FindFirst("email")?.Value;

        return Ok(new
        {
            authenticated = user.Identity?.IsAuthenticated ?? false,
            userId,
            email,
            claims = user.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}
