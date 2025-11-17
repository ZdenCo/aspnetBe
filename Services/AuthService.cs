using System.Security.Claims;
using System.Text.Json;

public class AuthService
{
    public ITokenData GetTokenDataFromContext(ClaimsPrincipal context)
    {

        var userMetadata = JsonSerializer.Deserialize<IUserMetadata>(
                context.FindFirst("user_metadata")?.Value ?? "{}");

        var subject = context.FindFirst("sub")?.Value ?? context.FindFirst("oid")?.Value ?? userMetadata.sub;
        var email = context.FindFirst("email")?.Value ?? userMetadata.email;

        return new ITokenData
        {
            email = email,
            subject = subject
        };

    }
}