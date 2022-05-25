namespace MiniBank.Data.CurrencyCourseServices;

public class CurrencyCourseResponse
{
    public Dictionary<string, ValuteItem> Valute { get; set; }
}

public class ValuteItem
{
    public double Value { get; set; }
}