namespace BankingPoc.Data.Options;

public class AccountConfigOptions
{
    public decimal MaxDepositAmount { get; set; }
    public decimal MinAccountAmount { get; set; }
    public int MaxWithdrawPercentage { get; set; }
}