using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core.Users;
using MiniBank.Core.Users.Services;
using MiniBank.Web.Controllers.Users.Dto;

namespace MiniBank.Web.Controllers.Users;

[ApiController]
[Authorize]
[Route("[controller]")]
public class UserController
{
    private readonly IUserService _userService;
    
    private UserDto ConvertToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Login = user.Login,
            Email = user.Email
        };
    }

    public UserController(IUserService userService)
    {
        _userService = userService;
    }


    [HttpGet("{id}")]
    public async Task<UserDto> GetUserById(string id, CancellationToken token)
    {
        var user = await _userService.GetUserByIdAsync(id, token);

        return ConvertToUserDto(user);
    }
        
    [HttpGet]
    public async Task<IEnumerable<UserDto>> GetAll(CancellationToken token)
    {
        return (await _userService.GetAllUsersAsync(token))
            .Select(ConvertToUserDto);
    }
        

    [HttpPost]
    public Task Create(CreateUserDto user, CancellationToken token)
    {
        return _userService.CreateUserAsync(new User
        {
            Login = user.Login,
            Email = user.Email
        }, token);
    }


    [HttpPut("/{id}")]
    public Task Update(string id, UpdateUserDto user, CancellationToken token)
    {
        return _userService.UpdateUserAsync(new User
        {
            Id = id,
            Login = user.Login,
            Email = user.Email
        }, token);
    }


    [HttpDelete("/{id}")]
    public Task Delete(string id, CancellationToken token)
    {
        return _userService.DeleteUserAsync(id, token);
    }

}