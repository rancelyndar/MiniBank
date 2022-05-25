using MiniBank.Core;

namespace MiniBank.Web.Controllers.BankAccounts.Dto;

public class BankAccountDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public bool IsOpen { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
}