namespace CurrencyConverter.Services.Result
{
    public record HistoricalExchangeRateResult(OrderedDictionary<DateOnly, Dictionary<string, decimal>> Rates);
}