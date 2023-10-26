using BankingPoc.Services.Models;

namespace BankingPoc.Services.Interfaces;

public interface IUserService
{
    Task<UserViewDto> UpsertUser(UserUpsertRequestDto user);
    Task<ExtendedUserViewDto?> GetUser(int userId);
    Task DeleteUser(int userId);
}