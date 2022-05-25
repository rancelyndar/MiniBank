using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniBank.Core.CurrencyConverterServices;
using MiniBank.Data.CurrencyCourseServices;

namespace MiniBank.Web.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class CurrencyConverterController : ControllerBase
{
    private readonly ICurrencyConverterService _currencyConverterService;

    public CurrencyConverterController(ICurrencyConverterService currencyConverterService)
    {
        this._currencyConverterService = currencyConverterService;
    }

    [HttpGet()]
    public Task<decimal> Get(string fromCurrency, string toCurrency, decimal amount, CancellationToken token)
    {
        return _currencyConverterService.ConvertCurrencyAsync(fromCurrency, toCurrency, amount, token);
    }
}