namespace CurrencyConverter.Services.Result
{
    public record ExchangeRateResult(Dictionary<string, decimal> Rates);
}