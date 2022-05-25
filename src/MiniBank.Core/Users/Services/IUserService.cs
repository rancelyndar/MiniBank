namespace MiniBank.Core.Users.Services;

public interface IUserService
{
    Task<User> GetUserByIdAsync(string id, CancellationToken token);
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken token);
    Task CreateUserAsync(User user, CancellationToken token);
    Task UpdateUserAsync(User user, CancellationToken token);
    Task DeleteUserAsync(string id, CancellationToken token);
}