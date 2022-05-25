namespace MiniBank.Web.Controllers.TransactionsHistories.Dto;

public class TransactionsHistoryDto
{
    public string Id { get; set; }
    public decimal Amount { get; set; }
    public string FromAccountId { get; set; }
    public string ToAccountId { get; set; }
}