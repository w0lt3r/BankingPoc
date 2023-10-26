using BankingPoc.Services.Interfaces;
using BankingPoc.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankingPoc.Controllers;

[ApiController]
[Route("[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    [HttpPost("deposit")]
    public async Task<AccountViewDto> Deposit(AccountAmountUpdateRequestDto payload)
    {
        return await _accountService.Deposit(payload);
    }
    
    [HttpPost("withdraw")]
    public async Task<AccountViewDto> Withdraw(AccountAmountUpdateRequestDto payload)
    {
        return await _accountService.Withdraw(payload);
    }
    
    [HttpDelete("{accountId}")]
    public async Task Delete(int accountId)
    {
        await _accountService.DeleteAccount(accountId);
    }
    
    [HttpPost]
    public async Task Create(AccountCreateRequestDto payload)
    {
        await _accountService.CreateAccount(payload);
    }
}