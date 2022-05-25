using MiniBank.Core.BankAccounts;

namespace MiniBank.Core.TransactionsHistories;

public class Transaction
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public string FromAccountId { get; set; }
    public string ToAccountId { get; set; }
}