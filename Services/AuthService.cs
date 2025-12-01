using System.Security.Claims;
using System.Text.Json;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IUserService _userService;

    public AuthService(ILogger<AuthService> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public Task<User> GetAuthUser(ClaimsPrincipal context)
    {
        var tokenData = GetTokenDataFromContext(context);

        if (tokenData.email == null)
        {
            throw new Exception("Email claim is missing");
        }

        return _userService.EnsureUserExistsAsync(tokenData.email);
    }

    private ITokenData GetTokenDataFromContext(ClaimsPrincipal context)
    {
        var userMetadata = JsonSerializer.Deserialize<IUserMetadata>(
                context.FindFirst("user_metadata")?.Value ?? "{}");
        _logger.LogInformation("email: {Email}", userMetadata.email ?? context.FindFirst("email")?.Value);

        var email = context.FindFirst("email")?.Value ?? userMetadata.email;


        return new ITokenData
        {
            email = email,
        };

    }
}