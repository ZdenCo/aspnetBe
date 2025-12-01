using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User> EnsureUserExistsAsync(string email)
    {

        var user = await GetUserByEmailAsync(email);

        _logger.LogInformation($"Checking existence for user with Email: {email}");
        _logger.LogInformation(user != null
            ? "User found in database."
            : "User not found in database. Creating new user...");

        if (user != null)
        {
            return user;
        }

        return await CreateUserAsync(email);
    }

    public Task<User?> GetUserByEmailAsync(string email)
    {
        return _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User> CreateUserAsync(string email)
    {
        var user = new User
        {
            GuidId = Guid.NewGuid(),
            Email = email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return user;
    }
}