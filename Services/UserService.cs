using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User> EnsureUserExistsAsync(string issuer, string subject)
    {

        var user = await GetUserByIssuerAndSubjectAsync(issuer, subject);

        _logger.LogInformation($"Checking existence for user with Issuer: {issuer}, Subject: {subject}");
        _logger.LogInformation(user != null
            ? "User found in database."
            : "User not found in database. Creating new user...");

        if (user != null)
        {
            return user;
        }

        return await CreateUserAsync(issuer, subject);
    }

    public async Task<User?> GetUserByIssuerAndSubjectAsync(string issuer, string subject)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Issuer == issuer && u.Subject == subject);
    }

    public async Task<User> CreateUserAsync(string issuer, string subject)
    {
        var user = new User
        {
            Issuer = issuer,
            Subject = subject,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }
}