using System.Security.Claims;

public interface IAuthService
{
    Task<User> GetAuthUser(ClaimsPrincipal token);
}