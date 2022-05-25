using System.Net.Http.Json;
using MiniBank.Core.CurrencyConverterServices;

namespace MiniBank.Data.CurrencyCourseServices;

public class CurrencyCourseService : ICurrencyCourseService
{
    private readonly HttpClient _httpClient;

    public CurrencyCourseService(HttpClient httpClient)
    {
        this._httpClient = httpClient;
    }

    public async Task<decimal> GetCurrencyCourseAsync(string currencyCode)
    {
        var response = await _httpClient.GetFromJsonAsync<CurrencyCourseResponse>("");

        response.Valute["RUB"] = new ValuteItem() {Value = 1};
        
        return (decimal) response.Valute[currencyCode].Value;
    }
}