using BankingPoc.Services.Models;

namespace BankingPoc.Services.Interfaces;

public interface IAccountService
{
    Task<AccountViewDto> Withdraw(AccountAmountUpdateRequestDto payload);
    Task<AccountViewDto> Deposit(AccountAmountUpdateRequestDto payload);
    Task<AccountViewDto> CreateAccount(AccountCreateRequestDto payload);
    Task DeleteAccount(int accountId);
}