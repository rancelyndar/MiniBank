namespace MiniBank.Core.BankAccounts.Models;

public class UpdateAccountModel
{
    public string Id { get; set; }
    public bool IsOpen { get; set; }
    public decimal Amount { get; set; }
}