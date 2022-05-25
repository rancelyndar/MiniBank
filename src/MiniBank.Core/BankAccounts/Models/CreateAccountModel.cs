namespace MiniBank.Core.BankAccounts.Models;

public class CreateAccountModel
{
    public string UserId { get; set; } 
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
}