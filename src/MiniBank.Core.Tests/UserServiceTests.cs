using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentValidation;
using MiniBank.Core.Users;
using MiniBank.Core.Users.Repositories;
using MiniBank.Core.Users.Services;
using MiniBank.Core.Users.Validators;
using Moq;
using Xunit;

namespace MiniBank.Core.Tests;

public class UserServiceTests
{
    private readonly IUserService _userService;
    private readonly Mock<IUnitOfWork> _fakeUnitOfWork;
    private readonly Mock<IUserRepository> _fakeUserRepository;
    private readonly IValidator<User> _userValidator;

    public UserServiceTests()
    {
        _fakeUnitOfWork = new Mock<IUnitOfWork>();
        _fakeUserRepository = new Mock<IUserRepository>();
        _userValidator = new UserValidator();

        _userService = new UserService(_fakeUserRepository.Object, _fakeUnitOfWork.Object, _userValidator);
    }
    
    [Fact]
    public void GetUserByIdAsync_UserDoesNotExist_ShouldThrowException()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        string fakeId = "Id";

        var exception = Assert.
            ThrowsAsync<ValidationException>(() => _userService.GetUserByIdAsync(fakeId, CancellationToken.None));
        
        Assert.Equal($"Пользователя с id={fakeId} не существует", exception.Result.Message);
    }

    [Fact]
    public void GetUserByIdAsync_SuccessPath_ShouldReturnUser()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeUserRepository
            .Setup(repository => repository.GetUserByIdAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(new User());
        var fakeId = "Id";

        var user = _userService.GetUserByIdAsync(fakeId, CancellationToken.None);
        
        Assert.IsType<User>(user.Result);
    }

    [Fact]
    public void GetAllUsersAsync_SuccessPath_ShouldReturnListOfUsers()
    {
        _fakeUserRepository
            .Setup(repository => repository.GetAllUsersAsync(CancellationToken.None))
            .ReturnsAsync(new List<User>());

        var users = _userService.GetAllUsersAsync(CancellationToken.None);
        
        Assert.IsType<List<User>>(users.Result);
    }

    [Fact]
    public void CreateUserAsync_WithEmptyLogin_ShouldThrowException()
    {
        var user = new User() {Email = "Email", Login = string.Empty, Id = "Id"};

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _userService.CreateUserAsync(user, CancellationToken.None));
        
        Assert.Equal("Login должен быть не пустым", exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void CreateUserAsync_WithLongLogin_ShouldThrowException()
    {
        var user = new User() {Email = "Email", Login = "loginloginloginloginlogin", Id = "Id"};

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _userService.CreateUserAsync(user, CancellationToken.None));
        
        Assert.Equal("Login не должен превышать 20 символов", exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void CreateUserAsync_WithEmptyEmail_ShouldThrowException()
    {
        var user = new User() {Email = string.Empty, Login = "Login", Id = "Id"};

        var exception = Assert
            .ThrowsAsync<FluentValidation.ValidationException>(() =>
                _userService.CreateUserAsync(user, CancellationToken.None));
        
        Assert.Equal("Email должен быть не пустым", exception.Result.Errors.Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void CreateUserAsync_SuccessPath_ShouldCallRepositoryMethodOnce()
    {
        _fakeUserRepository
            .Setup(repository => repository.CreateUserAsync(It.IsAny<User>(), CancellationToken.None));
        var user = new User() {Email = "Email", Login = "Login"};

        _userService.CreateUserAsync(user, CancellationToken.None);
        
        _fakeUserRepository.Verify(mock => mock.CreateUserAsync(user, CancellationToken.None), Times.Once);
        _fakeUnitOfWork.Verify(mock => mock.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void UpdateUserAsync_UserDoesNotExist_ShouldThrowException()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        var fakeUser = new User() {Id = "Id", Login = "Login", Email = "Email"};

        var exception = Assert.
            ThrowsAsync<ValidationException>(() => _userService.UpdateUserAsync(fakeUser, CancellationToken.None));
        
        Assert.Equal($"Пользователя с id={fakeUser.Id} не существует", exception.Result.Message);
    }
    
    [Fact]
    public void UpdateUserAsync_WithEmptyLogin_ShouldThrowException()
    {
        var fakeUser = new User() {Id = "Id", Login = string.Empty, Email = "Email"};

        var exception = Assert.
            ThrowsAsync<FluentValidation.ValidationException>(() => 
                _userService.UpdateUserAsync(fakeUser, CancellationToken.None));
        
        Assert.Equal($"Login должен быть не пустым", exception.Result.Errors
            .Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void UpdateUserAsync_WithEmptyEmail_ShouldThrowException()
    {
        var fakeUser = new User() {Id = "Id", Login = "Login", Email = string.Empty};

        var exception = Assert.
            ThrowsAsync<FluentValidation.ValidationException>(() => 
                _userService.UpdateUserAsync(fakeUser, CancellationToken.None));
        
        Assert.Equal($"Email должен быть не пустым", exception.Result.Errors
            .Select(x => $"{x.ErrorMessage}").First());
    }
    
    [Fact]
    public void UpdateUserAsync_WithLongLogin_ShouldThrowException()
    {
        var fakeUser = new User() {Id = "Id", Login = "loginloginloginloginlogin", Email = "Email"};

        var exception = Assert.
            ThrowsAsync<FluentValidation.ValidationException>(() => _userService.UpdateUserAsync(fakeUser, CancellationToken.None));
        
        Assert.Equal($"Login не должен превышать 20 символов", exception.Result.Errors
            .Select(x => $"{x.ErrorMessage}").First());
    }

    [Fact]
    public void UpdateUserAsync_SuccessPath_ShouldCallRepositoryMethodOnce()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeUserRepository
            .Setup(repository => repository.UpdateUserAsync(It.IsAny<User>(), CancellationToken.None));
        var user = new User() {Email = "Email", Login = "Login"};

        _userService.UpdateUserAsync(user, CancellationToken.None);
        
        _fakeUserRepository.Verify(mock => mock.UpdateUserAsync(user, CancellationToken.None), Times.Once);
        _fakeUnitOfWork.Verify(mock => mock.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public void DeleteUserAsync_UserDoesNotExist_ShouldThrowException()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        var fakeId = "Id";

        var exception = Assert
            .ThrowsAsync<ValidationException>(() => _userService.DeleteUserAsync(fakeId, CancellationToken.None));

        Assert.Equal($"Пользователя с id={fakeId} не существует", exception.Result.Message);
    }
    
    [Fact]
    public void DeleteUserAsync_UserHasAccounts_ShouldThrowException()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeUserRepository
            .Setup(repository => repository.UserHasAccountsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        var fakeId = "Id";

        var exception = Assert
            .ThrowsAsync<ValidationException>(() => _userService.DeleteUserAsync(fakeId, CancellationToken.None));

        Assert.Equal("У пользователя есть незакрытые аккаунты", exception.Result.Message);
    }

    [Fact]
    public void DeleteUserAsync_SuccessPath_ShouldCallRepositoryMethodOnce()
    {
        _fakeUserRepository
            .Setup(repository => repository.UserExistsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(true);
        _fakeUserRepository
            .Setup(repository => repository.UserHasAccountsAsync(It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(false);
        _fakeUserRepository
            .Setup(repository => repository.DeleteUserAsync(It.IsAny<string>(), CancellationToken.None));
        var fakeId = "Id";

        _userService.DeleteUserAsync(fakeId, CancellationToken.None);
        
        _fakeUserRepository.Verify(mock => mock.DeleteUserAsync(fakeId, CancellationToken.None), Times.Once);
        _fakeUnitOfWork.Verify(mock => mock.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}