namespace MiniBank.Core.Users.Repositories;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(string id, CancellationToken token);
    Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken token);
    Task CreateUserAsync(User user, CancellationToken token);
    Task UpdateUserAsync(User user, CancellationToken token);
    Task DeleteUserAsync(string id, CancellationToken token);
    Task<bool> UserExistsAsync(string id, CancellationToken token);
    Task<bool> UserHasAccountsAsync(string id, CancellationToken token);
}