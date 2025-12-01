using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProperAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;

    public UsersController(IUserService userService, IAuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    // GET /api/users/me
    [HttpGet("me")]
    [Authorize] // requires valid Bearer token
    public async Task<ActionResult<User>> GetUser()
    {
        var dbUser = HttpContext.Items["User"] as User;

        if (dbUser == null)
        {
            throw new UnauthorizedAccessException("User not found in database");
        }        

        return Ok(dbUser);
    }
}
