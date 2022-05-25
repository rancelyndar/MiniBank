namespace MiniBank.Core.CurrencyConverterServices;

public class CurrencyConverterService : ICurrencyConverterService
{
    private readonly ICurrencyCourseService _currencyCourseService;

    public CurrencyConverterService(ICurrencyCourseService currencyCourseService)
    {
        this._currencyCourseService = currencyCourseService;
    }
    
    public async Task<decimal> ConvertCurrencyAsync(string fromCurency, string toCurrency, decimal amount, CancellationToken token)
    {
        if (amount < 0)
        {
            throw new ValidationException("Параметр amount не может быть отрицательным");
        }
        var result = Math.Round(amount * await _currencyCourseService.GetCurrencyCourseAsync(fromCurency) / 
                     await _currencyCourseService.GetCurrencyCourseAsync(toCurrency), 2);
        return result;
    }
}