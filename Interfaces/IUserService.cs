public interface IUserService
{
    Task<User> EnsureUserExistsAsync(string email);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(string email);
}