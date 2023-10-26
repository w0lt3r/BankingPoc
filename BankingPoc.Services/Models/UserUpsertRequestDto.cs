namespace BankingPoc.Services.Models;

public class UserUpsertRequestDto
{
    public int? UserId { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
}