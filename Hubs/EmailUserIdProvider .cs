using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Text.Json;

public class EmailUserIdProvider : IUserIdProvider
{
    private readonly ILogger<EmailUserIdProvider> _logger;
    public EmailUserIdProvider(ILogger<EmailUserIdProvider> logger)
    {
        _logger = logger;
    }
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.Email)?.Value
            ?? connection.User?.FindFirst("email")?.Value;
    }
}
