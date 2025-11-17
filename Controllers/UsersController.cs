using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProperAuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;
    private readonly AuthService _authService;

    public UsersController(UserService userService, ILogger<UsersController> logger, AuthService authService)
    {
        _userService = userService;
        _logger = logger;
        _authService = authService;
    }
    // GET /api/users/me
    [HttpGet("me")]
    [Authorize] // requires valid Bearer token
    public async Task<IActionResult> GetUser()
    {
        var ctx = HttpContext.User;
        _logger.LogInformation("Getting user information from token...");

        var tokenData = _authService.GetTokenDataFromContext(ctx);
        _logger.LogInformation($"User: {tokenData}");

        var subject = tokenData.subject;
        _logger.LogInformation($"Subject: {subject}");

        var issuer = tokenData.issuer;
        _logger.LogInformation($"Issuer: {issuer}");

        if (subject == null || issuer == null)
        {
           throw new UnauthorizedAccessException("Missing sub or iss claim");
        }

        var dbUser = await _userService.GetUserByIssuerAndSubjectAsync(issuer,subject);
        _logger.LogInformation($"Database User: {dbUser}");

        if (dbUser == null)
        {
            throw new UnauthorizedAccessException("User not found in database");
        }        

        return Ok(dbUser);
    }
}
