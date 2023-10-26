using BankingPoc.Data.Entities;
using BankingPoc.Data.Interfaces;
using BankingPoc.Services.Interfaces;
using BankingPoc.Services.Models;

namespace BankingPoc.Services;

public class UserService: IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UserViewDto> UpsertUser(UserUpsertRequestDto payload)
    {
        User user;
        if (payload.UserId is null or 0)
        {
            user = new User
            {
                Name = payload.Name,
                LastName = payload.LastName
            };
            user = await _unitOfWork.GetRepository<User>().Insert(user);
        }
        else
        {
            user = await _unitOfWork.GetRepository<User>().Get(payload.UserId);
            if (user is not null)
            {
                user.Name = payload.Name;
                user.LastName = payload.LastName;
                await _unitOfWork.GetRepository<User>().Update(user);
            }
            else
            {
                throw new InvalidOperationException($"The user does not exist");
            }
        }

        await _unitOfWork.SaveChanges();
        return new UserViewDto
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName
        };
    }

    public async Task DeleteUser(int userId)
    {
        var users = await _unitOfWork
            .GetRepository<User>()
            .Get(u => u.Id == userId, includeProperties: "Accounts");
        var user = users?.FirstOrDefault();
        if (user is not null)
        {
            if (user.Accounts is not null)
            {
                await _unitOfWork.GetRepository<Account>().DeleteRange(user.Accounts);
            }
            await _unitOfWork.GetRepository<User>().Delete(user);
            await _unitOfWork.SaveChanges();
        }
        else
        {
            throw new InvalidOperationException($"The user does not exist");
        }
    }
    
    public async Task<ExtendedUserViewDto?> GetUser(int userId)
    {
        var users = await _unitOfWork
            .GetRepository<User>()
            .Get(u => u.Id == userId, includeProperties: "Accounts");
        var user = users?.FirstOrDefault();
        if (user is null) return null;
        
        return new ExtendedUserViewDto
        {
            Id = user.Id,
            Name = user.Name,
            LastName = user.LastName,
            Accounts = user.Accounts is null? Array.Empty<AccountViewDto>() : user.Accounts.Select(a => new AccountViewDto
            {
                Label = a.Label,
                Amount = a.Amount,
                AccountId = a.Id
            })
        };
    }
}