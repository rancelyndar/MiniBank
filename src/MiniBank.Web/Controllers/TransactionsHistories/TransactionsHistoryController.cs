using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core.TransactionsHistories.Services;
using MiniBank.Web.Controllers.TransactionsHistories.Dto;

namespace MiniBank.Web.Controllers.TransactionsHistories;

[ApiController]
[Authorize]
[Route("[controller]")]
public class TransactionsHistoryController
{
    private readonly ITransactionsHistoryService _transactionsHistoryService;

    public TransactionsHistoryController(ITransactionsHistoryService transactionsHistoryService)
    {
        _transactionsHistoryService = transactionsHistoryService;
    }
    
    
    [HttpGet]
    public async Task<IEnumerable<TransactionsHistoryDto>> GetAll(CancellationToken token)
    {
        return (await _transactionsHistoryService.GetAllTransactionsAsync(token))
            .Select(transaction => new TransactionsHistoryDto()
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                FromAccountId = transaction.FromAccountId,
                ToAccountId = transaction.ToAccountId
            });
    }
}