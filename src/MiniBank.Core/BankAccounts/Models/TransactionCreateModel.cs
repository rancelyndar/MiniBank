namespace MiniBank.Core.BankAccounts.Models;

public class TransactionCreateModel
{
    public decimal Amount { get; set; }
    public string FromAccountId { get; set; }
    public string ToAccountId { get; set; }
}