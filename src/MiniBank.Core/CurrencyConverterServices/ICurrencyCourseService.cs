namespace MiniBank.Core.CurrencyConverterServices;

public interface ICurrencyCourseService
{
    Task<decimal> GetCurrencyCourseAsync(string currencyCode);
}