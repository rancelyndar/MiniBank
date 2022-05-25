namespace MiniBank.Core.CurrencyConverterServices;

public interface ICurrencyConverterService
{
    Task<decimal> ConvertCurrencyAsync(string fromCurrency, string toCurrency, decimal amount, CancellationToken token);
}