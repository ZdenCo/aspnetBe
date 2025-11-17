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

    public async Task<User> EnsureUserExistsAsync(string email, string subject)
    {

        var user = await GetUserByEmailAndSubjectAsync(email, subject);

        _logger.LogInformation($"Checking existence for user with Email: {email}, Subject: {subject}");
        _logger.LogInformation(user != null
            ? "User found in database."
            : "User not found in database. Creating new user...");

        if (user != null)
        {
            return user;
        }

        return await CreateUserAsync(email, subject);
    }

    public async Task<User?> GetUserByEmailAndSubjectAsync(string email, string subject)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.Subject == subject);
    }

    public async Task<User> CreateUserAsync(string email, string subject)
    {
        var user = new User
        {
            Email = email,
            Subject = subject,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }
}