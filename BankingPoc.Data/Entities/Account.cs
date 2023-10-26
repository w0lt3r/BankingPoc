namespace BankingPoc.Data.Entities;

public class Account
{
    public int Id { get; set; }
    public string Label { get; set; }
    public decimal Amount { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
}