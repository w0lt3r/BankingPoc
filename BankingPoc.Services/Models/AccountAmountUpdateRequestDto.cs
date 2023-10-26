namespace BankingPoc.Services.Models;

public class AccountAmountUpdateRequestDto
{
    public int AccountId { get; set; }
    public decimal Amount { get; set; }
}