using MiniBank.Core;

namespace MiniBank.Web.Controllers.BankAccounts.Dto;

public class CreateAccountDto
{
    public string UserId { get; set; } 
    public Currency Currency { get; set; }
    public decimal Amount { get; set; }
}