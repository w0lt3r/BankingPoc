namespace BankingPoc.Services.Models;

public class AccountViewDto
{
    public int AccountId { get; set; }
    public string Label { get; set; }
    public decimal Amount { get; set; }
}