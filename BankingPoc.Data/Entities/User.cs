namespace BankingPoc.Data.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public List<Account> Accounts { get; set; }
}