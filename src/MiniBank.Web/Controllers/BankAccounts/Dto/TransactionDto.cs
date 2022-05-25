namespace MiniBank.Web.Controllers.BankAccounts.Dto;

public class TransactionDto
{
    public decimal Amount { get; set; }
    public string FromAccountId { get; set; }
    public string ToAccountId { get; set; }
}