namespace BankingPoc.Services.Models;

public class ExtendedUserViewDto: UserViewDto
{
    public IEnumerable<AccountViewDto> Accounts { get; set; }
}