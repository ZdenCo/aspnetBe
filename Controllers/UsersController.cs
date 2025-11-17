using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProperAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly AuthService _authService;

    public UsersController(UserService userService, AuthService authService)
    {
        _userService = userService;
        _authService = authService;
    }
    // GET /api/users/me
    [HttpGet("me")]
    [Authorize] // requires valid Bearer token
    public async Task<ActionResult<User>> GetUser()
    {
        var ctx = HttpContext.User;

        var tokenData = _authService.GetTokenDataFromContext(ctx);

        var subject = tokenData.subject;
        var email = tokenData.email;

        var dbUser = await _userService.GetUserByEmailAndSubjectAsync(email,subject);

        if (dbUser == null)
        {
            throw new UnauthorizedAccessException("User not found in database");
        }        

        return Ok(dbUser);
    }
}
