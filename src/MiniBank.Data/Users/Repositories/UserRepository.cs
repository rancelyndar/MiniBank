using Microsoft.EntityFrameworkCore;
using MiniBank.Core.BankAccounts.Repositories;
using MiniBank.Core.BankAccounts.Services;

namespace MiniBank.Core.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MiniBankContext _context;
    private readonly IBankAccountRepository _bankAccountRepository;

    public UserRepository(IBankAccountRepository bankAccountRepository, MiniBankContext context)
    {
        _bankAccountRepository = bankAccountRepository;
        _context = context;
    }
    
    public async Task<User> GetUserByIdAsync(string id, CancellationToken token)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, token);

        return new User
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email
        };
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken token)
    {
        return await _context.Users
            .AsNoTracking()
            .Select(user => new User()
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email
        })
            .ToListAsync(token);
    }

    public async Task CreateUserAsync(User user, CancellationToken token)
    {
        var newUser = new UserDbModel
        {
            Id = Guid.NewGuid().ToString(),
            Login = user.Login,
            Email = user.Email
        };
            
        await _context.Users.AddAsync(newUser, token);
    }

    public async Task UpdateUserAsync(User user, CancellationToken token)
    {
        var userToUpdate = await _context.Users.FirstOrDefaultAsync(it => it.Id == user.Id, token);
        
        userToUpdate.Login = user.Login;
        userToUpdate.Email = user.Email;
        
    }

    public async Task DeleteUserAsync(string id, CancellationToken token)
    {
        var userToDelete = await _context.Users.FirstOrDefaultAsync(user => user.Id == id, token);
        
        _context.Users.Remove(userToDelete);
    }

    public async Task<bool> UserHasAccountsAsync(string id, CancellationToken token)
    {
        return (await _bankAccountRepository.GetUserAccountsAsync(id, token)).Count() != 0;
    }

    public async Task<bool> UserExistsAsync(string id, CancellationToken token)
    {
        return await _context.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, token) != null;
    }

}