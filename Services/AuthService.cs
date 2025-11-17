using System.Security.Claims;
using System.Text.Json;

public class AuthService
{
    public ITokenData GetTokenDataFromContext(ClaimsPrincipal context)
    {
        var issuer = context.FindFirst("iss")?.Value;

        var userMetadata = JsonSerializer.Deserialize<IUserMetadata>(
                context.FindFirst("user_metadata")?.Value ?? "{}");

        var subject = context.FindFirst("sub")?.Value ?? context.FindFirst("oid")?.Value ?? userMetadata.sub;

        return new ITokenData
        {
            issuer = issuer,
            subject = subject
        };

    }
}