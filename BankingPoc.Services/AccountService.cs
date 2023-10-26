using BankingPoc.Data.Entities;
using BankingPoc.Data.Interfaces;
using BankingPoc.Data.Options;
using BankingPoc.Services.Interfaces;
using BankingPoc.Services.Models;
using Microsoft.Extensions.Options;

namespace BankingPoc.Services;

public class AccountService: IAccountService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AccountConfigOptions _options;
    public AccountService(IUnitOfWork unitOfWork,
        IOptions<AccountConfigOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }
    
    public async Task<AccountViewDto> Withdraw(AccountAmountUpdateRequestDto payload)
    {
        var account = await _unitOfWork.GetRepository<Account>().Get(payload.AccountId);
        if (account is not null)
        {
            if ((payload.Amount/account.Amount)*100 > _options.MaxWithdrawPercentage)
            {
                throw new ArgumentOutOfRangeException(nameof(payload.Amount),$"The amount cannot exceed the {_options.MaxWithdrawPercentage}% of the balance");
            }
            
            if (account.Amount - payload.Amount < _options.MinAccountAmount)
            {
                throw new ArgumentOutOfRangeException(nameof(payload.Amount),$"The remaining balance cannot be less than {_options.MinAccountAmount}");
            }

            account.Amount -= payload.Amount;
            await _unitOfWork.GetRepository<Account>().Update(account);
            await _unitOfWork.SaveChanges();
            return new AccountViewDto
            {
                Label = account.Label,
                Amount = account.Amount,
                AccountId = account.Id,
            };
        }
        throw new InvalidOperationException($"The account does not exist");
    }

    public async Task<AccountViewDto> Deposit(AccountAmountUpdateRequestDto payload)
    {
        if (payload.Amount <= _options.MaxDepositAmount)
        {
            var account = await _unitOfWork.GetRepository<Account>().Get(payload.AccountId);
            if (account is not null)
            {
                account.Amount += payload.Amount;
                await _unitOfWork.GetRepository<Account>().Update(account);
                await _unitOfWork.SaveChanges();
                return new AccountViewDto
                {
                    Label = account.Label,
                    Amount = account.Amount,
                    AccountId = account.Id,
                };
            }
            throw new InvalidOperationException($"The account does not exist");
        }
        throw new ArgumentOutOfRangeException(nameof(payload.Amount), $"The amount cannot exceed {_options.MaxDepositAmount}");
    }

    public async Task DeleteAccount(int accountId)
    {
        var account = await _unitOfWork.GetRepository<Account>().Get(accountId);
        if (account is not null)
        {
            await _unitOfWork.GetRepository<Account>().Delete(account);
            await _unitOfWork.SaveChanges();
        }
        else
        {
            throw new InvalidOperationException($"The account does not exist");
        }
    }
    
    public async Task<AccountViewDto> CreateAccount(AccountCreateRequestDto payload)
    {
        var account = new Account();
        account.Label = payload.Label;
        account.Amount = _options.MinAccountAmount;
        
        var user = await _unitOfWork.GetRepository<User>().Get(payload.UserId);
        if (user is not null)
        {
            account.UserId = user.Id;
            var createdAccount = await _unitOfWork.GetRepository<Account>().Insert(account);
            await _unitOfWork.SaveChanges();
            return new AccountViewDto
            {
                Label = createdAccount.Label,
                Amount = createdAccount.Amount,
                AccountId = createdAccount.Id,
            };
        }
        throw new InvalidOperationException($"The user does not exist");
    }
}