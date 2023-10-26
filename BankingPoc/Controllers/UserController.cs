using BankingPoc.Services.Interfaces;
using BankingPoc.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingPoc.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId}")]
    public async Task<ExtendedUserViewDto> Get(int userId)
    {
        return await _userService.GetUser(userId);
    }
    
    [HttpPut]
    public async Task<UserViewDto> Upsert(UserUpsertRequestDto userId)
    {
        return await _userService.UpsertUser(userId);
    }
    
    [HttpDelete("{userId}")]
    public async Task Upsert(int userId)
    {
        await _userService.DeleteUser(userId);
    }
}