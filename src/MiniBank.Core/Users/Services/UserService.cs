using FluentValidation;
using MiniBank.Core.Users.Repositories;

namespace MiniBank.Core.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<User> _userValidator;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork, IValidator<User> userValidator)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _userValidator = userValidator;
    }

    private async Task ValidateAndThrow(string id, CancellationToken token)
    {
        if (!await _userRepository.UserExistsAsync(id, token))
        {
            throw new ValidationException($"Пользователя с id={id} не существует");
        }
    }

    private async Task ValidateAndThrowDelete(string id, CancellationToken token)
    {
        if (!await _userRepository.UserExistsAsync(id, token))
        {
            throw new ValidationException($"Пользователя с id={id} не существует");
        }

        if (await _userRepository.UserHasAccountsAsync(id, token))
        {
            throw new ValidationException("У пользователя есть незакрытые аккаунты");
        }
    }
    
    public async Task<User> GetUserByIdAsync(string id, CancellationToken token)
    {
        await ValidateAndThrow(id, token);
        
        return await _userRepository.GetUserByIdAsync(id, token);
    }

    public Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken token)
    {
        return _userRepository.GetAllUsersAsync(token);
    }

    public async Task CreateUserAsync(User user, CancellationToken token)
    {
        _userValidator.ValidateAndThrow(user);
            
        await _userRepository.CreateUserAsync(user, token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task UpdateUserAsync(User user, CancellationToken token)
    {
        _userValidator.ValidateAndThrow(user);
        await ValidateAndThrow(user.Id, token);

        await _userRepository.UpdateUserAsync(user, token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task DeleteUserAsync(string id, CancellationToken token)
    {
        await ValidateAndThrowDelete(id, token);
        
        await _userRepository.DeleteUserAsync(id, token);
        await _unitOfWork.SaveChangesAsync(token);
    }
}